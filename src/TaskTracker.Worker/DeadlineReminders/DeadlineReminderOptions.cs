namespace TaskTracker.Worker.DeadlineReminders;

public sealed class DeadlineReminderOptions
{
    public TimeSpan LeadTime { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromMinutes(2);
    public int BatchSize { get; set; } = 100;
    public int MaxConcurrency { get; set; } = 8;
}
