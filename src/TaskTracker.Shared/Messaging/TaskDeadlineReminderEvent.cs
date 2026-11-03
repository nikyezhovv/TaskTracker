namespace TaskTracker.Contracts;

public sealed record TaskDeadlineReminderEvent(
    Guid TaskId,
    Guid OwnerId,
    string TaskTitle,
    DateTimeOffset Deadline,
    DateTimeOffset ReminderTime);
