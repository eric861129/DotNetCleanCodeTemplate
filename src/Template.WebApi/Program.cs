//#if (useJwt)
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//#endif
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
//#if (useJwt)
using Microsoft.IdentityModel.Tokens;
//#endif
using System.Threading.RateLimiting;
using Template.Application;
using Template.Infrastructure;
//#if (includeOrders)
using Template.WebApi.Endpoints;
//#endif
//#if (useDatabase)
using Template.WebApi.Health;
//#endif
using Template.WebApi.Middleware;
using Template.WebApi.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
var healthChecks = builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck("ready", () => HealthCheckResult.Healthy(), tags: ["ready"]);
//#if (useDatabase)
healthChecks.AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);
//#endif
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddOptions<RateLimitingOptions>()
    .Bind(builder.Configuration.GetSection(RateLimitingOptions.SectionName))
    .Validate(options => options.IsValid(), "RateLimiting options require positive PermitLimit and WindowSeconds, and non-negative QueueLimit.")
    .ValidateOnStart();

builder.Services.AddRateLimiter();
builder.Services.AddOptions<RateLimiterOptions>()
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
//#if (useJwt)
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(options => options.IsValid(), $"Jwt options require Issuer, Audience, and a SigningKey with at least {JwtOptions.MinimumSigningKeyLength} characters.")
    .ValidateOnStart();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is required.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization();
//#endif

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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

app.Run();

public partial class Program;
