//#if (useJwt)
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//#endif
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
//#if (useJwt)
using Microsoft.IdentityModel.Tokens;
//#endif
using System.Threading.RateLimiting;
using Template.Application;
using Template.Infrastructure;
//#if (useDatabase)
using Template.WebApi.Health;
//#endif
using Template.WebApi.Options;

namespace Template.WebApi.Extensions;

public static class WebApiServiceCollectionExtensions
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddWebApiHealthChecks();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Template API",
                Version = "v1",
                Description = "Versioned Clean Architecture API sample."
            });
        });
        services.AddProblemDetails();
        services.AddWebApiRateLimiting(configuration);
        services.AddWebApiObservability(configuration);
        //#if (useJwt)
        services.AddJwtAuthentication(configuration);
        //#endif

        return services;
    }

    private static IServiceCollection AddWebApiHealthChecks(this IServiceCollection services)
    {
        var healthChecks = services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck("ready", () => HealthCheckResult.Healthy(), tags: ["ready"]);
        //#if (useDatabase)
        healthChecks.AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);
        //#endif

        return services;
    }

    private static IServiceCollection AddWebApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .Validate(options => options.IsValid(), "RateLimiting options require positive PermitLimit and WindowSeconds, and non-negative QueueLimit.")
            .ValidateOnStart();

        services.AddRateLimiter();
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<RateLimitingOptions>>((options, rateLimitingOptionsAccessor) =>
            {
                var rateLimitingOptions = rateLimitingOptionsAccessor.Value;

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("global", limiterOptions =>
                {
                    limiterOptions.PermitLimit = rateLimitingOptions.PermitLimit;
                    limiterOptions.Window = TimeSpan.FromSeconds(rateLimitingOptions.WindowSeconds);
                    limiterOptions.QueueLimit = rateLimitingOptions.QueueLimit;
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.AutoReplenishment = true;
                });
            });

        return services;
    }

    //#if (useJwt)
    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => options.IsValid(), $"Jwt options require Issuer, Audience, and a SigningKey with at least {JwtOptions.MinimumSigningKeyLength} characters.")
            .ValidateOnStart();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is required.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        return services;
    }
    //#endif
}
