namespace TaskTracker.Persistence.Entities;

public sealed class TaskReminderEntity
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset? Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? DeadlineReminderSentAt { get; set; }
    public DateTimeOffset? DeadlineReminderLeaseUntil { get; set; }
    public Guid? DeadlineReminderLeaseId { get; set; }
}
