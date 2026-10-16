using System.Diagnostics;

using Microsoft.AspNetCore.Http;

using Serilog.Context;

namespace TaskTracker.Shared.Observability;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext ctx)
    {
        var id = ctx.Request.Headers[CorrelationIdHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(id) || id.Length > 64)
        {
            id = Activity.Current?.TraceId.ToString()
                 ?? ActivityTraceId.CreateRandom().ToString();
        }

        ctx.Response.OnStarting(() =>
        {
            ctx.Response.Headers[CorrelationIdHeader] = id;
            return Task.CompletedTask;
        });

        Activity.Current?.SetTag("correlation_id", id);

        using (LogContext.PushProperty("CorrelationId", id))
        {
            await next(ctx);
        }
    }
}
