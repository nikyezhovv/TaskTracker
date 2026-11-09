namespace TaskTracker.Persistence;

public interface ITaskDeadlineReminderRepository
{
    Task<IReadOnlyList<ClaimedTaskReminder>> ClaimDueAsync(
        int batchSize,
        TimeSpan leadTime,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken);

    Task<bool> MarkSentAsync(Guid taskId, Guid leaseId, CancellationToken cancellationToken);
}
