namespace TaskTracker.Worker.DeadlineReminders;

internal static partial class DeadlineReminderLog
{
    [LoggerMessage(1201, LogLevel.Information, "Claimed {ReminderCount} deadline reminders")]
    public static partial void Claimed(ILogger logger, int reminderCount);

    [LoggerMessage(1202, LogLevel.Error, "Deadline reminder polling failed")]
    public static partial void PollFailed(ILogger logger, Exception exception);

    [LoggerMessage(1203, LogLevel.Warning, "Reminder {ReminderId} was published after its lease was lost")]
    public static partial void LeaseLost(ILogger logger, Guid reminderId);

    [LoggerMessage(1204, LogLevel.Error, "Failed to publish deadline reminder {ReminderId}; it can be retried after its lease expires")]
    public static partial void PublishFailed(ILogger logger, Exception exception, Guid reminderId);
}
