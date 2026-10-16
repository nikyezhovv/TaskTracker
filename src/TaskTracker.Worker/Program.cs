using Serilog;
using Serilog.Formatting.Compact;

using TaskTracker.Shared.Observability;
using TaskTracker.Worker;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddObservability("TaskTracker.Worker");

    builder.Services.AddHostedService<Worker>();

    var app = builder.Build();
    app.UseObservability();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
