namespace TaskTracker.Contracts;

public sealed record TaskDeadlineReminderEvent(
    Guid ReminderId,
    Guid TaskId,
    Guid OwnerId,
    string TaskTitle,
    DateTimeOffset Deadline,
    DateTimeOffset ReminderTime);
