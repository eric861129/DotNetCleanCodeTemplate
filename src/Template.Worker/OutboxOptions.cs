namespace Template.Worker;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int PollingIntervalSeconds { get; init; } = 5;

    public int BatchSize { get; init; } = 20;
}
