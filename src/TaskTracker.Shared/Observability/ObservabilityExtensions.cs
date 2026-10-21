using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace TaskTracker.Shared.Observability;

public static class ObservabilityExtensions
{
    public static TBuilder AddObservability<TBuilder>(
        this TBuilder builder,
        string serviceName,
        string[]? additionalSources = null,
        string[]? additionalMeters = null)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddSerilogLogging(serviceName);
        builder.AddOpenTelemetryObservability(serviceName, additionalSources, additionalMeters);
        builder.Services.AddHealth();

        return builder;
    }

    public static WebApplication UseObservability(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseRequestLogging();
        app.MapPrometheusScrapingEndpoint("/metrics");
        app.MapHealth();

        return app;
    }
}
