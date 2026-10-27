using System.Text.Json;
using System.Text.Json.Serialization;

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
        services.AddSwaggerGen();
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddCommonsServices();
        return services;
    }
}
