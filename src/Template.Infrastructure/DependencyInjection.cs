//#if (useDatabase)
using Microsoft.EntityFrameworkCore;
//#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Application.Common;
//#if (includeOrders)
using Template.Application.Orders;
using Template.Infrastructure.InMemory;
//#endif
//#if (useDatabase)
using Template.Infrastructure.Options;
using Template.Infrastructure.Persistence;
//#endif

namespace Template.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //#if (includeOrders)
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<IOutboxMessageRepository, InMemoryOutboxMessageRepository>();
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
        //#endif

        //#if (useDatabase)
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(options => IsSupportedDatabaseProvider(options.Provider), "Database:Provider must be SqlServer or Sqlite.")
            .ValidateOnStart();

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(databaseOptions.Provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        //#endif

        return services;
    }

    //#if (useDatabase)
    private static bool IsSupportedDatabaseProvider(string provider)
    {
        return string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase);
    }
    //#endif
}
