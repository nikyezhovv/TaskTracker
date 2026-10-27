namespace TaskTracker.Api.Implementations.Repositories.Entities;

public sealed class MeetingLinkEntity
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Position { get; set; }
    public MeetingEntity Meeting { get; set; } = null!;
}
