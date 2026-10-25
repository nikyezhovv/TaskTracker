namespace TaskTracker.Api.Models;

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = "Todo";
    public string Priority { get; set; } = "Medium";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
