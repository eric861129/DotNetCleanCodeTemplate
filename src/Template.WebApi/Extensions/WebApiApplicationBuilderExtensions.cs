using Microsoft.AspNetCore.Diagnostics.HealthChecks;
//#if (includeOrders)
using Template.WebApi.Endpoints;
//#endif
using Template.WebApi.Middleware;

namespace Template.WebApi.Extensions;

public static class WebApiApplicationBuilderExtensions
{
    public static WebApplication UseWebApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Template API v1");
            });
        }

        app.UseCorrelationId();
        app.UseSecurityHeaders();
        app.UseRequestLogging();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseRouting();
        //#if (useJwt)
        app.UseAuthentication();
        app.UseAuthorization();
        //#endif
        app.UseRateLimiter();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("live")
        }).AllowAnonymous().RequireRateLimiting("global");
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready")
        }).AllowAnonymous().RequireRateLimiting("global");
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready")
        }).AllowAnonymous().RequireRateLimiting("global");
        //#if (includeOrders)
        app.MapOrderEndpoints();
        //#endif

        return app;
    }
}
