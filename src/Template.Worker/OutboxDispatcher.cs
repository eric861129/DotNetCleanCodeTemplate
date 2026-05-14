using Microsoft.Extensions.Options;
using Template.Application.Common;

namespace Template.Worker;

public sealed class OutboxDispatcher(
    IOutboxMessageRepository outboxMessageRepository,
    IOutboxMessageDispatcher dispatcher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IOptions<OutboxOptions> options,
    ILogger<OutboxDispatcher> logger)
{
    public async Task DispatchPendingMessagesAsync(CancellationToken cancellationToken)
    {
        var batchSize = Math.Max(1, options.Value.BatchSize);
        var messages = await outboxMessageRepository.GetPendingAsync(batchSize, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await dispatcher.DispatchAsync(message, cancellationToken);
                message.MarkProcessed(timeProvider.GetUtcNow());
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox message {MessageId} dispatch failed", message.Id);
                message.MarkFailed(exception.Message);
            }
        }

        if (messages.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
