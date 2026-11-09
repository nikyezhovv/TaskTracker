using Microsoft.AspNetCore.SignalR;

using TaskTracker.Contracts;

namespace TaskTracker.Notifications.Notifications;

public sealed class SignalRNotificationSink(IHubContext<NotificationHub> hubContext) : INotificationSink
{
    public Task SendTaskDeadlineReminderAsync(TaskDeadlineReminderEvent reminderEvent, CancellationToken cancellationToken)
    {
        var notification = new
        {
            @event = "TaskDeadlineApproaching",
            title = "Приближается дедлайн",
            message = $"Дедлайн задачи '{reminderEvent.TaskTitle}' скоро наступит.",
            data = new
            {
                notificationId = reminderEvent.ReminderId,
                taskId = reminderEvent.TaskId,
                deadline = reminderEvent.Deadline
            }
        };

        return hubContext.Clients.User(reminderEvent.OwnerId.ToString())
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }
}
