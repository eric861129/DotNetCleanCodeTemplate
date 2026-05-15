using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Template.Infrastructure.Observability;

namespace Template.WebApi.Extensions;

public static class WebApiObservabilityExtensions
{
    public static IServiceCollection AddWebApiObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var observabilityOptions = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>()
            ?? new ObservabilityOptions();

        services.AddOptions<ObservabilityOptions>()
            .Bind(configuration.GetSection(ObservabilityOptions.SectionName))
            .Validate(options => options.IsValid(), "Observability options require ServiceName, ServiceVersion, and a valid OTLP endpoint when enabled.")
            .ValidateOnStart();

        if (!observabilityOptions.Enabled)
        {
            return services;
        }

        var openTelemetryBuilder = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: observabilityOptions.ServiceName,
                serviceVersion: observabilityOptions.ServiceVersion));

        if (observabilityOptions.Tracing.Enabled)
        {
            openTelemetryBuilder.WithTracing(tracing =>
            {
                tracing
                    .AddSource(AppDiagnostics.ActivitySourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                //#if (useDatabase)
                tracing.AddEntityFrameworkCoreInstrumentation();
                //#endif
                tracing.AddConfiguredExporters(observabilityOptions);
            });
        }

        if (observabilityOptions.Metrics.Enabled)
        {
            openTelemetryBuilder.WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(AppDiagnostics.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                metrics.AddConfiguredExporters(observabilityOptions);
            });
        }

        return services;
    }

    private static TracerProviderBuilder AddConfiguredExporters(
        this TracerProviderBuilder builder,
        ObservabilityOptions options)
    {
        if (options.ConsoleExporter.Enabled)
        {
            builder.AddConsoleExporter();
        }

        if (options.OtlpExporter.Enabled)
        {
            builder.AddOtlpExporter(otlpOptions =>
            {
                if (!string.IsNullOrWhiteSpace(options.OtlpExporter.Endpoint))
                {
                    otlpOptions.Endpoint = new Uri(options.OtlpExporter.Endpoint);
                }
            });
        }

        return builder;
    }

    private static MeterProviderBuilder AddConfiguredExporters(
        this MeterProviderBuilder builder,
        ObservabilityOptions options)
    {
        if (options.ConsoleExporter.Enabled)
        {
            builder.AddConsoleExporter();
        }

        if (options.OtlpExporter.Enabled)
        {
            builder.AddOtlpExporter(otlpOptions =>
            {
                if (!string.IsNullOrWhiteSpace(options.OtlpExporter.Endpoint))
                {
                    otlpOptions.Endpoint = new Uri(options.OtlpExporter.Endpoint);
                }
            });
        }

        return builder;
    }
}
