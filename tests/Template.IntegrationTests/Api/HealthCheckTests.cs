using System.Net;
using Template.IntegrationTests.Support;

namespace Template.IntegrationTests.Api;

public sealed class HealthCheckTests : IDisposable
{
    private readonly TemplateWebApplicationFactory _factory = new();

    [Fact]
    public async Task LiveHealthCheckReturnsHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReadyHealthCheckReturnsHealthyWhenDatabaseCanConnect()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
