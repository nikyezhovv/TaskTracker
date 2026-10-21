using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TaskTracker.Shared.Observability;

public static class OpenTelemetryExtensions
{
    public static IHostApplicationBuilder AddOpenTelemetryObservability(
        this IHostApplicationBuilder builder,
        string serviceName,
        string[]? additionalSources = null,
        string[]? additionalMeters = null)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName)
                .AddEnvironmentVariableDetector())
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.RecordException = true;
                        o.Filter = ctx => !ObservabilityDefaults.NoisePaths
                            .Any(p => ctx.Request.Path.StartsWithSegments(p));
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();

                if (additionalSources is { Length: > 0 })
                {
                    tracing.AddSource(additionalSources);
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();

                if (additionalMeters is { Length: > 0 })
                {
                    metrics.AddMeter(additionalMeters);
                }
            });

        return builder;
    }
}
