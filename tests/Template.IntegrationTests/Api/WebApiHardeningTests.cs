using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
//#if (useDatabase)
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
//#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//#if (useDatabase)
using Template.Infrastructure.Persistence;
//#endif

namespace Template.IntegrationTests.Api;

public sealed class WebApiHardeningTests : IAsyncLifetime
{
    //#if (useDatabase)
    private readonly SqliteConnection _connection = new("Data Source=CleanCodeTemplateHardeningTests;Mode=Memory;Cache=Shared");
    //#endif
    private WebApplicationFactory<Program> _factory = null!;

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
        await using var factory = CreateFactory(new Dictionary<string, string?>
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

    public async Task InitializeAsync()
    {
        //#if (useDatabase)
        await _connection.OpenAsync();
        //#endif
        _factory = CreateFactory(new Dictionary<string, string?>());
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        //#if (useDatabase)
        await _connection.DisposeAsync();
        //#endif
    }

    private WebApplicationFactory<Program> CreateFactory(IReadOnlyDictionary<string, string?> extraConfiguration)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    var configuration = new Dictionary<string, string?>
                    {
                        //#if (useDatabase)
                        ["Database:Provider"] = "Sqlite",
                        ["ConnectionStrings:DefaultConnection"] = _connection.ConnectionString
                        //#endif
                    };

                    foreach (var item in extraConfiguration)
                    {
                        configuration[item.Key] = item.Value;
                    }

                    configurationBuilder.AddInMemoryCollection(configuration);
                });

                //#if (useDatabase)
                builder.ConfigureTestServices(services =>
                {
                    var dbContextDescriptors = services
                        .Where(descriptor =>
                            descriptor.ServiceType == typeof(DbContextOptions)
                            || descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>)
                            || descriptor.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration", StringComparison.Ordinal) == true)
                        .ToList();

                    foreach (var descriptor in dbContextDescriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

                    using var provider = services.BuildServiceProvider();
                    using var scope = provider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                });
                //#endif
            });
    }
}
