using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Host
{
    public static class OpenApiExtentions
    {
        public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
        {
            return services.AddSwaggerGen(options =>
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
    }
}
