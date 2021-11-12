using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Haland.DotNetTrace;
using Serilog;

namespace Host;

public partial class Startup
{
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
