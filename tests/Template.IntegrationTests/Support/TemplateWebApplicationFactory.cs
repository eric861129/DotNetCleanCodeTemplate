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

namespace Template.IntegrationTests.Support;

public sealed class TemplateWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _environment;
    private readonly IReadOnlyDictionary<string, string?> _extraConfiguration;
    private readonly Action<IServiceCollection>? _configureTestServices;
    //#if (useDatabase)
    private readonly SqliteConnection _connection;
    //#endif

    public TemplateWebApplicationFactory(
        IReadOnlyDictionary<string, string?>? extraConfiguration = null,
        string environment = "Testing",
        Action<IServiceCollection>? configureTestServices = null)
    {
        _environment = environment;
        _extraConfiguration = extraConfiguration ?? new Dictionary<string, string?>();
        _configureTestServices = configureTestServices;
        //#if (useDatabase)
        _connection = new SqliteConnection($"Data Source=CleanCodeTemplateTests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        _connection.Open();
        //#endif
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var configuration = new Dictionary<string, string?>
            {
                //#if (useDatabase)
                ["Database:Provider"] = "Sqlite",
                ["ConnectionStrings:DefaultConnection"] = _connection.ConnectionString
                //#endif
            };

            foreach (var item in _extraConfiguration)
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

        if (_configureTestServices is not null)
        {
            builder.ConfigureTestServices(_configureTestServices);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        //#if (useDatabase)
        if (disposing)
        {
            _connection.Dispose();
        }
        //#endif
    }
}
