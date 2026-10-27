using TaskTracker.Api.Implementations.Repositories;
using TaskTracker.Api.Implementations.Services;

namespace TaskTracker.Api.Implementations.HostConfiguration;

public static class CommonsServicesConfiguration
{
    public static IServiceCollection AddCommonsServices(this IServiceCollection services)
    {
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
