namespace TaskTracker.Api.Implementations.Repositories.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();
    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    public ICollection<MeetingEntity> OrganizedMeetings { get; set; } = new List<MeetingEntity>();
    public ICollection<MeetingParticipantEntity> MeetingParticipations { get; set; } = new List<MeetingParticipantEntity>();
}
