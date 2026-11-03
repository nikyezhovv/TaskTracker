using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TaskTracker.Persistence;

public static class TaskDeadlineReminderPersistenceExtensions
{
    public static IServiceCollection AddTaskDeadlineReminderPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContextFactory<TaskReminderDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton<ITaskDeadlineReminderRepository, TaskDeadlineReminderRepository>();
        return services;
    }
}
