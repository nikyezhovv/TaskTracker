using TaskTracker.Contracts;

namespace TaskTracker.Notifications.Notifications;

public interface INotificationSink
{
    Task SendTaskDeadlineReminderAsync(TaskDeadlineReminderEvent reminderEvent, CancellationToken cancellationToken);
}
