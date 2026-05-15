using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Template.Infrastructure.Observability;

public static class AppDiagnostics
{
    public const string ActivitySourceName = "CleanCodeTemplate";
    public const string MeterName = "CleanCodeTemplate";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> OutboxDispatchSuccessCounter = Meter.CreateCounter<long>(
        "outbox.dispatch.success.count",
        description: "Number of successfully dispatched outbox messages.");
    private static readonly Counter<long> OutboxDispatchFailureCounter = Meter.CreateCounter<long>(
        "outbox.dispatch.failure.count",
        description: "Number of failed outbox message dispatch attempts.");
    private static readonly Histogram<double> OutboxDispatchDuration = Meter.CreateHistogram<double>(
        "outbox.dispatch.duration.ms",
        unit: "ms",
        description: "Outbox message dispatch duration in milliseconds.");
    private static readonly Counter<long> OutboxBatchMessagesCounter = Meter.CreateCounter<long>(
        "outbox.batch.messages.count",
        description: "Number of outbox messages loaded for dispatch.");

    private static long _outboxPendingCount;
    private static long _outboxFailedCount;

    static AppDiagnostics()
    {
        Meter.CreateObservableGauge(
            "outbox.pending.count",
            () => Interlocked.Read(ref _outboxPendingCount),
            description: "Current number of pending outbox messages observed by the worker.");

        Meter.CreateObservableGauge(
            "outbox.failed.count",
            () => Interlocked.Read(ref _outboxFailedCount),
            description: "Current number of pending outbox messages that already failed at least once.");
    }

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind);
    }

    public static void RecordOutboxBatch(long messageCount)
    {
        OutboxBatchMessagesCounter.Add(messageCount);
    }

    public static void RecordOutboxDispatchSuccess(string messageType, double durationMilliseconds)
    {
        OutboxDispatchSuccessCounter.Add(1, CreateMessageTypeTag(messageType));
        OutboxDispatchDuration.Record(durationMilliseconds, CreateMessageTypeTag(messageType), CreateDispatchStatusTag("success"));
    }

    public static void RecordOutboxDispatchFailure(string messageType, double durationMilliseconds)
    {
        OutboxDispatchFailureCounter.Add(1, CreateMessageTypeTag(messageType));
        OutboxDispatchDuration.Record(durationMilliseconds, CreateMessageTypeTag(messageType), CreateDispatchStatusTag("failure"));
    }

    public static void RecordOutboxBacklog(long pendingCount, long failedCount)
    {
        Interlocked.Exchange(ref _outboxPendingCount, pendingCount);
        Interlocked.Exchange(ref _outboxFailedCount, failedCount);
    }

    private static KeyValuePair<string, object?> CreateMessageTypeTag(string messageType)
    {
        return new KeyValuePair<string, object?>("message.type", messageType);
    }

    private static KeyValuePair<string, object?> CreateDispatchStatusTag(string status)
    {
        return new KeyValuePair<string, object?>("dispatch.status", status);
    }
}
