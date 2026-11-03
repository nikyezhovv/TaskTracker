using Microsoft.EntityFrameworkCore;

using TaskTracker.Persistence.Entities;

namespace TaskTracker.Persistence;

public sealed class TaskDeadlineReminderRepository(
    IDbContextFactory<TaskReminderDbContext> dbContextFactory) : ITaskDeadlineReminderRepository
{
    public async Task<IReadOnlyList<ClaimedTaskReminder>> ClaimDueAsync(
        int batchSize,
        TimeSpan leadTime,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var reminderCutoff = now.Add(leadTime);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var candidateTaskIds = await dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.Deadline.HasValue
                && task.Deadline.Value > now
                && task.Deadline.Value <= reminderCutoff
                && task.Status != "Done"
                && task.DeadlineReminderSentAt == null
                && (task.DeadlineReminderLeaseUntil == null || task.DeadlineReminderLeaseUntil <= now))
            .OrderBy(task => task.Deadline)
            .ThenBy(task => task.Id)
            .Select(task => task.Id)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);

        if (candidateTaskIds.Length == 0)
        {
            return Array.Empty<ClaimedTaskReminder>();
        }

        var leaseId = Guid.NewGuid();
        var leaseUntil = now.Add(leaseDuration);
        Guid? nullableLeaseId = leaseId;
        DateTimeOffset? nullableLeaseUntil = leaseUntil;

        await dbContext.Tasks
            .Where(task => candidateTaskIds.Contains(task.Id)
                && task.Deadline.HasValue
                && task.Deadline.Value > now
                && task.Deadline.Value <= reminderCutoff
                && task.Status != "Done"
                && task.DeadlineReminderSentAt == null
                && (task.DeadlineReminderLeaseUntil == null || task.DeadlineReminderLeaseUntil <= now))
            .ExecuteUpdateAsync(update => update
                .SetProperty(task => task.DeadlineReminderLeaseId, nullableLeaseId)
                .SetProperty(task => task.DeadlineReminderLeaseUntil, nullableLeaseUntil), cancellationToken);

        return await dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.DeadlineReminderLeaseId == leaseId && task.DeadlineReminderSentAt == null)
            .Select(task => new ClaimedTaskReminder(
                task.Id,
                task.OwnerId,
                task.Title,
                task.Deadline!.Value,
                leaseId))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<bool> MarkSentAsync(Guid taskId, Guid leaseId, CancellationToken cancellationToken)
    {
        var sentAt = DateTimeOffset.UtcNow;
        DateTimeOffset? nullableSentAt = sentAt;

        var updatedRows = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using (updatedRows)
        {
            return await updatedRows.Tasks
                .Where(task => task.Id == taskId
                    && task.DeadlineReminderLeaseId == leaseId
                    && task.DeadlineReminderSentAt == null)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(task => task.DeadlineReminderSentAt, nullableSentAt)
                    .SetProperty(task => task.DeadlineReminderLeaseId, (Guid?)null)
                    .SetProperty(task => task.DeadlineReminderLeaseUntil, (DateTimeOffset?)null), cancellationToken) == 1;
        }
    }
}
