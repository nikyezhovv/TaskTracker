using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;
using TaskTracker.Api.Data;
using TaskTracker.Api.Endpoints;
using TaskTracker.Shared.Observability;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddObservability("TaskTracker.Api");
    builder.Services.AddOpenApi();
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"
            )
        )
    );

    var app = builder.Build();
    app.UseObservability();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapTaskEndpoints();
    }

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
