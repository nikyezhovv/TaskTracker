using TaskTracker.Api.Implementations.Model;

namespace TaskTracker.Api.Implementations.Repositories.Entities;

public sealed class TaskEntity
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset? Deadline { get; set; }
    public DateTimeOffset? DeadlineReminderSentAt { get; set; }
    public DateTimeOffset? DeadlineReminderLeaseUntil { get; set; }
    public Guid? DeadlineReminderLeaseId { get; set; }
    public TaskState Status { get; set; } = TaskState.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public UserEntity Owner { get; set; } = null!;
    public ICollection<TaskTagAssignmentEntity> TagAssignments { get; set; } = new List<TaskTagAssignmentEntity>();
}
