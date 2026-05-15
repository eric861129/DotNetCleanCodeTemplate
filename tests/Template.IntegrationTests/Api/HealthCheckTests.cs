using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
//#if (useDatabase)
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Infrastructure.Persistence;
//#endif

namespace Template.IntegrationTests.Api;

public sealed class HealthCheckTests : IAsyncLifetime
{
    //#if (useDatabase)
    private readonly SqliteConnection _connection = new("Data Source=CleanCodeTemplateHealthTests;Mode=Memory;Cache=Shared");
    //#endif
    private WebApplicationFactory<Program> _factory = null!;

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

    public async Task InitializeAsync()
    {
        //#if (useDatabase)
        await _connection.OpenAsync();
        //#endif
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                //#if (useDatabase)
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:Provider"] = "Sqlite",
                        ["ConnectionStrings:DefaultConnection"] = _connection.ConnectionString
                    });
                });

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

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        //#if (useDatabase)
        await _connection.DisposeAsync();
        //#endif
    }
}
