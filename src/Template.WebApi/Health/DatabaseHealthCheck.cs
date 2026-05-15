using Microsoft.Extensions.Diagnostics.HealthChecks;
using Template.Infrastructure.Persistence;

namespace Template.WebApi.Health;

public sealed class DatabaseHealthCheck(AppDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("Database connection is ready.")
            : HealthCheckResult.Unhealthy("Database connection is not available.");
    }
}
