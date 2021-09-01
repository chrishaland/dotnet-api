using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Haland.DotNetTrace;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Repository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Host
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AddDatabase(services);
            AddOpenApiDocumentation(services);

            AddAuthorizationAndPolicies(services);
            AddOpenIdConnectAuthentication(services);


            services.AddTracing();
            services.AddControllers();
        }

        private void AddDatabase(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("Database");
            services.AddDbContext<Database>(options =>
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    options.UseSqlServer(connectionString);
                }
                else
                {
                    options.UseInMemoryDatabase("Database");
                }
            });
        }

        private void AddAuthorizationAndPolicies(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimTypes.Role, Configuration.GetValue<string>("oidc:roles:user"))
                    .Build();

                options.AddPolicy("admin", policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimTypes.Role, Configuration.GetValue<string>("oidc:roles:admin")));
            });
        }

        private void AddOpenIdConnectAuthentication(IServiceCollection services)
        {
            var audience = Configuration.GetValue<string>("oidc:audience");
            var authorityUri = Configuration.GetValue<string>("oidc:authorityUri");
            if (string.IsNullOrEmpty(authorityUri)) return;

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Audience = audience;
                options.Authority = authorityUri;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    RequireSignedTokens = true,
                    NameClaimType = Configuration.GetValue<string>("oidc:claim_types:name") ?? "name",
                    RoleClaimType = Configuration.GetValue<string>("oidc:claim_types:role") ?? "roles"
                };
            });
        }

        private void AddOpenApiDocumentation(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Provide value as: 'Bearer <id_token>'",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
                options.TagActionsBy(api =>
                {
                    var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
                    if (controllerActionDescriptor == null) throw new InvalidOperationException("Unable to determine tag for endpoint.");

                    var route = controllerActionDescriptor.AttributeRouteInfo?.Template;
                    if (route != null)
                    {
                        var parts = route.Split('/');
                        if (parts.Length > 1) return new[] { String.Join('/', parts.Take(parts.Length - 1)) };
                    }

                    return new[] { controllerActionDescriptor.ControllerName };
                });
                options.DocInclusionPredicate((name, api) => true);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseTracing();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
            
            app.UseReferrerPolicy(options => options.NoReferrer());

            app.UseRedirectValidation(options =>
            {
                options.AllowSameHostRedirectsToHttps();
                
                var authorityUri = Configuration.GetValue<string>("oidc:authorityUri");
                if (string.IsNullOrEmpty(authorityUri)) return;
                options.AllowedDestinations(authorityUri);
            });

            app.UseXContentTypeOptions();
            app.UseXRobotsTag(options => options.NoIndex().NoFollow());

            if (!env.IsDevelopment())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseExceptionHandler(app =>
            {
                app.Run(async context =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();

                    await Task.CompletedTask;
                    logger.LogError(exceptionHandler?.Error, "Application error:");
                });
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
                options.RoutePrefix = "docs";
            });

            app.UseCsp(options =>
            {
                options.BlockAllMixedContent();
                options.StyleSources(s => s.Self());
                options.FontSources(s => s.Self());
                options.FormActions(s => s.Self());
                options.FrameAncestors(s => s.Self());
                options.ImageSources(s => s.Self());
                options.ScriptSources(s => s.Self());
            });

            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
