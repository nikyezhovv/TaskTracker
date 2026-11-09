using Serilog;
using Serilog.Formatting.Compact;

using TaskTracker.Notifications.HostConfiguration;
using TaskTracker.Notifications.Notifications;
using TaskTracker.Shared.Observability;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddObservability("TaskTracker.Notifications");
    builder.Services.AddNotificationMessaging(builder.Configuration);

    var app = builder.Build();
    app.UseObservability();

    app.MapHub<NotificationHub>("/hubs/notifications");

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
