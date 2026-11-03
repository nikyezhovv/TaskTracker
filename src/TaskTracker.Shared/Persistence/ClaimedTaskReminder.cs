namespace TaskTracker.Persistence;

public sealed record ClaimedTaskReminder(
    Guid TaskId,
    Guid OwnerId,
    string TaskTitle,
    DateTimeOffset Deadline,
    Guid LeaseId);
