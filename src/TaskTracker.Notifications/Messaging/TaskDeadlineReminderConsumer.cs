using MassTransit;

using TaskTracker.Contracts;
using TaskTracker.Notifications.Notifications;

namespace TaskTracker.Notifications.Messaging;

public sealed class TaskDeadlineReminderConsumer(INotificationSink notificationSink) : IConsumer<TaskDeadlineReminderEvent>
{
    public Task Consume(ConsumeContext<TaskDeadlineReminderEvent> context) =>
        notificationSink.SendTaskDeadlineReminderAsync(context.Message, context.CancellationToken);
}
