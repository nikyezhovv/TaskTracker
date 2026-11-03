using MassTransit;

using Microsoft.Extensions.DependencyInjection;
using Testcontainers.RabbitMq;

using TaskTracker.Notifications.Messaging;
using TaskTracker.Notifications.Notifications;
using TaskTracker.Contracts;
using TaskTracker.Worker.Messaging;

namespace TaskTracker.IntegrationTests;

[TestFixture]
public sealed class TaskDeadlineReminderMessagingTests
{
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:4-management").Build();

    [OneTimeSetUp]
    public async Task StartRabbitMqAsync()
    {
        await _rabbitMqContainer.StartAsync();
    }

    [OneTimeTearDown]
    public async Task StopRabbitMqAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
    }

    [Test]
    public async Task PublishedReminderIsConsumedAndRoutedToItsOwner()
    {
        var notificationSink = new RecordingNotificationSink();
        var services = new ServiceCollection();
        services.AddSingleton<INotificationSink>(notificationSink);
        services.AddSingleton<ITaskDeadlineReminderPublisher, TaskDeadlineReminderPublisher>();
        services.AddMassTransit(registration =>
        {
            registration.AddConsumer<TaskDeadlineReminderConsumer>();
            registration.UsingRabbitMq((context, busConfiguration) =>
            {
                busConfiguration.Host(new Uri(_rabbitMqContainer.GetConnectionString()));
                busConfiguration.ConfigureEndpoints(context);
            });
        });

        await using var serviceProvider = services.BuildServiceProvider();
        var busControl = serviceProvider.GetRequiredService<IBusControl>();
        await busControl.StartAsync();

        try
        {
            var reminderEvent = new TaskDeadlineReminderEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Сдать проект",
                DateTimeOffset.UtcNow.AddMinutes(15),
                DateTimeOffset.UtcNow);

            await serviceProvider.GetRequiredService<ITaskDeadlineReminderPublisher>()
                .PublishAsync(reminderEvent, CancellationToken.None);

            var receivedEvent = await notificationSink.Received.Task.WaitAsync(TimeSpan.FromSeconds(15));

            Assert.That(receivedEvent, Is.EqualTo(reminderEvent));
        }
        finally
        {
            await busControl.StopAsync();
        }
    }

    private sealed class RecordingNotificationSink : INotificationSink
    {
        public TaskCompletionSource<TaskDeadlineReminderEvent> Received { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task SendTaskDeadlineReminderAsync(TaskDeadlineReminderEvent reminderEvent, CancellationToken cancellationToken)
        {
            Received.TrySetResult(reminderEvent);
            return Task.CompletedTask;
        }
    }
}
