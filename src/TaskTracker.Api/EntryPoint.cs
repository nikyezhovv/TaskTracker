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
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });

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
            options.UseRequestInterceptor("""
                (request) => {
                    const accessToken = window.localStorage.getItem("tasktracker_access_token");
                    if (accessToken) {
                        request.headers = request.headers || {};
                        request.headers["Authorization"] = `Bearer ${accessToken}`;
                    }
                    return request;
                }
                """);
            options.UseResponseInterceptor("""
                (response) => {
                    if (response.url.endsWith("/api/auth/login") && response.status === 200 && response.text) {
                        try {
                            const loginResult = JSON.parse(response.text);
                            if (loginResult.accessToken) {
                                window.localStorage.setItem("tasktracker_access_token", loginResult.accessToken);
                            }
                        } catch (_) { }
                    }
                    return response;
                }
                """);
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
