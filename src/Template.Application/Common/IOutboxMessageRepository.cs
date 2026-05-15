namespace Template.Application.Common;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);

    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken);

    Task<long> CountPendingAsync(CancellationToken cancellationToken);

    Task<long> CountFailedAsync(CancellationToken cancellationToken);
}
