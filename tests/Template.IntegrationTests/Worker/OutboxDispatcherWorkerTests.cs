using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Template.Application.Common;
using Template.Infrastructure.Persistence;
using Template.Worker;

namespace Template.IntegrationTests.Worker;

public sealed class OutboxDispatcherWorkerTests
{
    [Fact]
    public async Task DispatchPendingMessagesMarksThemProcessed()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=:memory:"));
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IOutboxMessageDispatcher, RecordingOutboxMessageDispatcher>();
        services.AddSingleton(TimeProvider.System);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.OpenConnectionAsync();
        await dbContext.Database.EnsureCreatedAsync();
        dbContext.OutboxMessages.Add(OutboxMessage.Create("OrderCreated", """{"orderId":"demo"}""", DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var worker = new OutboxDispatcher(
            scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>(),
            scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            TimeProvider.System,
            Options.Create(new OutboxOptions { BatchSize = 10, PollingIntervalSeconds = 1 }),
            NullLogger<OutboxDispatcher>.Instance);

        await worker.DispatchPendingMessagesAsync(CancellationToken.None);

        var message = await dbContext.OutboxMessages.SingleAsync();
        Assert.NotNull(message.ProcessedAt);
    }

    private sealed class RecordingOutboxMessageDispatcher : IOutboxMessageDispatcher
    {
        public Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
