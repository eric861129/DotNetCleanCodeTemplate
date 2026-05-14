using Template.Application.Common;

namespace Template.Worker;

public sealed class LoggingOutboxMessageDispatcher(ILogger<LoggingOutboxMessageDispatcher> logger)
    : IOutboxMessageDispatcher
{
    public Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Dispatching outbox message {MessageId} with type {MessageType}", message.Id, message.Type);
        return Task.CompletedTask;
    }
}
