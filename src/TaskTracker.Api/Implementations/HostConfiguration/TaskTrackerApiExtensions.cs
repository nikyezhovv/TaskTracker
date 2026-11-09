using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;

using Microsoft.EntityFrameworkCore;

using TaskTracker.Api.Implementations.Repositories;

namespace TaskTracker.Api.Implementations.HostConfiguration;

public static class TaskTrackerApiExtensions
{
    public static IServiceCollection AddTaskTrackerApi(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddControllers().AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        });
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes =
                    new Dictionary<string, IOpenApiSecurityScheme>
                    {
                        ["Bearer"] = new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT"
                        }
                    };

                foreach (var path in document.Paths)
                {
                    if (!path.Key.StartsWith(
                            "/api/tasks",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    foreach (var operation in path.Value.Operations!.Values)
                    {
                        operation.Security ??= [];
                        operation.Security.Add(new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                        });
                    }
                }

                return Task.CompletedTask;
            });
        });
        
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddCommonsServices();
        services.AddTaskTrackerAuth(configuration);

        return services;
    }
}
