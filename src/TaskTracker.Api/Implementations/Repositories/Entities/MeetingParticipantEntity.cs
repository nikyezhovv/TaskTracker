namespace TaskTracker.Api.Implementations.Repositories.Entities;

public sealed class MeetingParticipantEntity
{
    public Guid MeetingId { get; set; }
    public Guid UserId { get; set; }
    public string? ParticipantStatus { get; set; }
    public string? ParticipantLocation { get; set; }
    public DateTimeOffset AddedAt { get; set; }
    public MeetingEntity Meeting { get; set; } = null!;
    public UserEntity User { get; set; } = null!;
}
