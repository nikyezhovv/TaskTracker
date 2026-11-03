using MassTransit;

using TaskTracker.Contracts;

namespace TaskTracker.Worker.Messaging;

public sealed class TaskDeadlineReminderPublisher(IBus bus) : ITaskDeadlineReminderPublisher
{
    public Task PublishAsync(TaskDeadlineReminderEvent reminderEvent, CancellationToken cancellationToken) =>
        bus.Publish(reminderEvent, cancellationToken);
}
