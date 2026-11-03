namespace TaskTracker.Api.Implementations.Repositories.Entities;

public sealed class TaskTagAssignmentEntity
{
    public Guid TaskId { get; set; }
    public Guid TagId { get; set; }
    public TaskEntity Task { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
}
