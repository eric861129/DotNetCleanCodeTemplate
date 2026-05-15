using System.Net;
using System.Text.Json;
using Template.IntegrationTests.Support;

namespace Template.IntegrationTests.Api;

public sealed class WebApiHardeningTests : IDisposable
{
    private readonly TemplateWebApplicationFactory _factory = new();

    [Fact]
    public async Task ResponseIncludesGeneratedCorrelationId()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        Assert.False(string.IsNullOrWhiteSpace(response.Headers.GetValues("X-Correlation-Id").Single()));
    }

    [Fact]
    public async Task ResponsePreservesIncomingCorrelationId()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-Id", "integration-test-correlation-id");

        var response = await client.SendAsync(request);

        Assert.Equal("integration-test-correlation-id", response.Headers.GetValues("X-Correlation-Id").Single());
    }

    [Fact]
    public async Task ResponseIncludesSecurityHeaders()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
    }

    [Fact]
    public async Task RateLimiterReturnsTooManyRequestsWhenLimitIsExceeded()
    {
        using var factory = new TemplateWebApplicationFactory(new Dictionary<string, string?>
        {
            ["RateLimiting:PermitLimit"] = "1",
            ["RateLimiting:WindowSeconds"] = "60",
            ["RateLimiting:QueueLimit"] = "0"
        });
        var client = factory.CreateClient();

        var firstResponse = await client.GetAsync("/health/live");
        var secondResponse = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
    }

    [Fact]
    public async Task SwaggerV1DocumentIsAvailableInDevelopment()
    {
        using var factory = new TemplateWebApplicationFactory(environment: "Development");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var swaggerJson = await response.Content.ReadAsStringAsync();
        using var swaggerDocument = JsonDocument.Parse(swaggerJson);
        Assert.Equal("v1", swaggerDocument.RootElement.GetProperty("info").GetProperty("version").GetString());
        //#if (includeOrders)
        Assert.Contains("/api/v1/orders", swaggerJson, StringComparison.OrdinalIgnoreCase);
        //#endif
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
