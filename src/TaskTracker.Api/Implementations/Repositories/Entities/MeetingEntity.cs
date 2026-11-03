namespace TaskTracker.Api.Implementations.Repositories.Entities;

public sealed class MeetingEntity
{
    public Guid Id { get; set; }
    public Guid OrganizerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public UserEntity Organizer { get; set; } = null!;
    public ICollection<MeetingParticipantEntity> Participants { get; set; } = new List<MeetingParticipantEntity>();
    public ICollection<MeetingTagAssignmentEntity> TagAssignments { get; set; } = new List<MeetingTagAssignmentEntity>();
    public ICollection<MeetingLinkEntity> Links { get; set; } = new List<MeetingLinkEntity>();
}
