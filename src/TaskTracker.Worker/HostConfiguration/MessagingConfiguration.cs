using MassTransit;

using TaskTracker.Worker.Messaging;

namespace TaskTracker.Worker.HostConfiguration;

public static class MessagingConfiguration
{
    public static IServiceCollection AddWorkerMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(registration =>
        {
            registration.UsingRabbitMq((_, busConfiguration) =>
            {
                busConfiguration.Host(
                    configuration["RabbitMq:Host"] ?? "localhost",
                    configuration["RabbitMq:VirtualHost"] ?? "/",
                    hostConfiguration =>
                    {
                        hostConfiguration.Username(configuration["RabbitMq:Username"] ?? "guest");
                        hostConfiguration.Password(configuration["RabbitMq:Password"] ?? "guest");
                    });
            });
        });

        services.AddSingleton<ITaskDeadlineReminderPublisher, TaskDeadlineReminderPublisher>();
        return services;
    }
}
