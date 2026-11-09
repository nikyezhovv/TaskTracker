using TaskTracker.Contracts;

namespace TaskTracker.Worker.Messaging;

public interface ITaskDeadlineReminderPublisher
{
    Task PublishAsync(TaskDeadlineReminderEvent reminderEvent, CancellationToken cancellationToken);
}
