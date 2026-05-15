namespace Template.WebApi.Options;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int PermitLimit { get; init; } = 100;

    public int WindowSeconds { get; init; } = 60;

    public int QueueLimit { get; init; } = 0;

    public bool IsValid()
    {
        return PermitLimit > 0
            && WindowSeconds > 0
            && QueueLimit >= 0;
    }
}
