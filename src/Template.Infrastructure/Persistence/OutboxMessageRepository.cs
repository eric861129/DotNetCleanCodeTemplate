using Microsoft.EntityFrameworkCore;
using Template.Application.Common;

namespace Template.Infrastructure.Persistence;

public sealed class OutboxMessageRepository(AppDbContext dbContext) : IOutboxMessageRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        var pendingMessages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAt == null)
            .ToListAsync(cancellationToken);

        return pendingMessages
            .OrderBy(message => message.OccurredAt)
            .Take(batchSize)
            .ToList();
    }

    public Task<long> CountPendingAsync(CancellationToken cancellationToken)
    {
        return dbContext.OutboxMessages
            .LongCountAsync(message => message.ProcessedAt == null, cancellationToken);
    }

    public Task<long> CountFailedAsync(CancellationToken cancellationToken)
    {
        return dbContext.OutboxMessages
            .LongCountAsync(
                message => message.ProcessedAt == null && message.RetryCount > 0,
                cancellationToken);
    }
}
