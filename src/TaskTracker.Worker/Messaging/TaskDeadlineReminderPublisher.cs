using MassTransit;

using TaskTracker.Contracts;

namespace TaskTracker.Worker.Messaging;

public sealed class TaskDeadlineReminderPublisher(IPublishEndpoint publishEndpoint) : ITaskDeadlineReminderPublisher
{
    public Task PublishAsync(TaskDeadlineReminderEvent reminderEvent, CancellationToken cancellationToken) =>
        publishEndpoint.Publish(reminderEvent, cancellationToken);
}
