using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace TaskTracker.Shared.Observability;

public static class LoggingExtensions
{
    public static IHostApplicationBuilder AddSerilogLogging(this IHostApplicationBuilder builder, string serviceName)
    {
        builder.Services.AddSerilog((services, cfg) => cfg
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithSpan()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console(new RenderedCompactJsonFormatter()));

        return builder;
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(o =>
        {
            o.GetLevel = (http, _, ex) =>
                ex is not null || http.Response.StatusCode >= 500 ? LogEventLevel.Error
                : ObservabilityDefaults.NoisePaths.Any(p => http.Request.Path.StartsWithSegments(p)) ? LogEventLevel.Verbose
                : LogEventLevel.Information;
        });
    }
}
