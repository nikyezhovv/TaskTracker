using MassTransit;

using TaskTracker.Notifications.Messaging;
using TaskTracker.Notifications.Notifications;

namespace TaskTracker.Notifications.HostConfiguration;

public static class MessagingConfiguration
{
    public static IServiceCollection AddNotificationMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR();

        services.AddMassTransit(registration =>
        {
            registration.AddConsumer<TaskDeadlineReminderConsumer>();
            registration.UsingRabbitMq((context, busConfiguration) =>
            {
                busConfiguration.Host(
                    configuration["RabbitMq:Host"] ?? "localhost",
                    configuration["RabbitMq:VirtualHost"] ?? "/",
                    hostConfiguration =>
                    {
                        hostConfiguration.Username(configuration["RabbitMq:Username"] ?? "guest");
                        hostConfiguration.Password(configuration["RabbitMq:Password"] ?? "guest");
                    });

                busConfiguration.UseMessageRetry(retry => retry.Intervals(
                    TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15)));
                busConfiguration.ConfigureEndpoints(context);
            });
        });

        services.AddSingleton<INotificationSink, SignalRNotificationSink>();
        return services;
    }
}
