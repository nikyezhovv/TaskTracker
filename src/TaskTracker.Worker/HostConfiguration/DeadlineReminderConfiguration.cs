using TaskTracker.Persistence;
using TaskTracker.Worker.DeadlineReminders;

namespace TaskTracker.Worker.HostConfiguration;

public static class DeadlineReminderConfiguration
{
    public static IServiceCollection AddDeadlineReminderProcessing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing for TaskTracker.Worker.");

        services.AddTaskDeadlineReminderPersistence(connectionString);
        services.AddOptions<DeadlineReminderOptions>()
            .Bind(configuration.GetSection("DeadlineReminders"))
            .Validate(options => options.LeadTime > TimeSpan.Zero, "LeadTime must be positive.")
            .Validate(options => options.PollInterval > TimeSpan.Zero, "PollInterval must be positive.")
            .Validate(options => options.LeaseDuration > TimeSpan.Zero, "LeaseDuration must be positive.")
            .Validate(options => options.BatchSize > 0, "BatchSize must be positive.")
            .Validate(options => options.MaxConcurrency > 0, "MaxConcurrency must be positive.")
            .ValidateOnStart();
        services.AddHostedService<DeadlineReminderWorker>();

        return services;
    }
}
