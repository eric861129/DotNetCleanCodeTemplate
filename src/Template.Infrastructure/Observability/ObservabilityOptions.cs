namespace Template.Infrastructure.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public bool Enabled { get; init; }

    public string ServiceName { get; init; } = "CleanCodeTemplate";

    public string ServiceVersion { get; init; } = "1.0.0";

    public ObservabilitySignalOptions Tracing { get; init; } = new();

    public ObservabilitySignalOptions Metrics { get; init; } = new();

    public ObservabilityExporterOptions ConsoleExporter { get; init; } = new();

    public ObservabilityOtlpExporterOptions OtlpExporter { get; init; } = new();

    public bool IsValid()
    {
        if (!Enabled)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(ServiceName)
            && !string.IsNullOrWhiteSpace(ServiceVersion)
            && OtlpExporter.IsValid();
    }
}

public sealed class ObservabilitySignalOptions
{
    public bool Enabled { get; init; } = true;
}

public sealed class ObservabilityExporterOptions
{
    public bool Enabled { get; init; }
}

public sealed class ObservabilityOtlpExporterOptions
{
    public bool Enabled { get; init; }

    public string Endpoint { get; init; } = string.Empty;

    public bool IsValid()
    {
        return !Enabled
            || string.IsNullOrWhiteSpace(Endpoint)
            || Uri.TryCreate(Endpoint, UriKind.Absolute, out _);
    }
}
