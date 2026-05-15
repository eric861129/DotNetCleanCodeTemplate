using System.Diagnostics;
using Microsoft.Extensions.Options;
using Template.Application.Common;
using Template.Infrastructure.Observability;

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
        AppDiagnostics.RecordOutboxBatch(messages.Count);

        foreach (var message in messages)
        {
            using var activity = AppDiagnostics.StartActivity("outbox.dispatch", ActivityKind.Consumer);
            activity?.SetTag("messaging.message.id", message.Id);
            activity?.SetTag("messaging.message.type", message.Type);
            var startedAt = Stopwatch.GetTimestamp();

            try
            {
                await dispatcher.DispatchAsync(message, cancellationToken);
                message.MarkProcessed(timeProvider.GetUtcNow());
                var duration = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
                AppDiagnostics.RecordOutboxDispatchSuccess(message.Type, duration);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox message {MessageId} dispatch failed", message.Id);
                message.MarkFailed(exception.Message);
                var duration = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
                AppDiagnostics.RecordOutboxDispatchFailure(message.Type, duration);
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                activity?.AddException(exception);
            }
        }

        if (messages.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var pendingCount = await outboxMessageRepository.CountPendingAsync(cancellationToken);
        var failedCount = await outboxMessageRepository.CountFailedAsync(cancellationToken);
        AppDiagnostics.RecordOutboxBacklog(pendingCount, failedCount);
    }
}
