using Serilog;
using Serilog.Formatting.Compact;

using TaskTracker.Api.Implementations.HostConfiguration;
using TaskTracker.Shared.Observability;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddObservability("TaskTracker.Api");
    builder.Services.AddTaskTrackerApi(builder.Configuration);

    var app = builder.Build();
    await app.Services.ApplyDatabaseMigrationsAsync();

    app.UseObservability();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/openapi/v1.json",
                "TaskTracker API v1"
            );
        });
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
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
