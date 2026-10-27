namespace TaskTracker.Api.Implementations.Repositories.Entities;

public sealed class MeetingTagAssignmentEntity
{
    public Guid MeetingId { get; set; }
    public Guid TagId { get; set; }
    public MeetingEntity Meeting { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
}
