using Template.Application.Common;

namespace Template.Infrastructure.InMemory;

public sealed class InMemoryOutboxMessageRepository : IOutboxMessageRepository
{
    private readonly List<OutboxMessage> _messages = [];

    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        _messages.Add(message);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        IReadOnlyList<OutboxMessage> messages = _messages
            .Where(message => message.ProcessedAt is null)
            .OrderBy(message => message.OccurredAt)
            .Take(batchSize)
            .ToList();

        return Task.FromResult(messages);
    }

    public Task<long> CountPendingAsync(CancellationToken cancellationToken)
    {
        var count = _messages.LongCount(message => message.ProcessedAt is null);

        return Task.FromResult(count);
    }

    public Task<long> CountFailedAsync(CancellationToken cancellationToken)
    {
        var count = _messages.LongCount(message => message.ProcessedAt is null && message.RetryCount > 0);

        return Task.FromResult(count);
    }
}
