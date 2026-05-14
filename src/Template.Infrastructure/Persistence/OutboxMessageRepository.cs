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
}
