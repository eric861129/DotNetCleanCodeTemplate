//#if (useJwt)
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//#endif
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
//#if (useJwt)
using Microsoft.IdentityModel.Tokens;
//#endif
using Template.Application;
using Template.Infrastructure;
//#if (includeOrders)
using Template.WebApi.Endpoints;
//#endif
//#if (useDatabase)
using Template.WebApi.Health;
//#endif
//#if (useJwt)
using Template.WebApi.Options;
//#endif

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

app.UseExceptionHandler();
app.UseHttpsRedirection();
//#if (useJwt)
app.UseAuthentication();
app.UseAuthorization();
//#endif

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("live")
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
}).AllowAnonymous();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
}).AllowAnonymous();
//#if (includeOrders)
app.MapOrderEndpoints();
//#endif

app.Run();

public partial class Program;
