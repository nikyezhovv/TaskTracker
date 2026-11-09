using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using TaskTracker.Contracts;
using TaskTracker.Persistence;
using TaskTracker.Worker.Messaging;

namespace TaskTracker.Worker.DeadlineReminders;

public sealed class DeadlineReminderWorker(
    ITaskDeadlineReminderRepository reminderRepository,
    ITaskDeadlineReminderPublisher reminderPublisher,
    IOptions<DeadlineReminderOptions> options,
    ILogger<DeadlineReminderWorker> logger) : BackgroundService
{
    private readonly DeadlineReminderOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var claimedReminders = await reminderRepository.ClaimDueAsync(
                    _options.BatchSize,
                    _options.LeadTime,
                    _options.LeaseDuration,
                    stoppingToken);

                if (claimedReminders.Count > 0)
                {
                    DeadlineReminderLog.Claimed(logger, claimedReminders.Count);
                    await Parallel.ForEachAsync(
                        claimedReminders,
                        new ParallelOptions
                        {
                            CancellationToken = stoppingToken,
                            MaxDegreeOfParallelism = _options.MaxConcurrency
                        },
                        PublishReminderAsync);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                DeadlineReminderLog.PollFailed(logger, exception);
            }

            await Task.Delay(_options.PollInterval, stoppingToken);
        }
    }

    private async ValueTask PublishReminderAsync(ClaimedTaskReminder claimedReminder, CancellationToken cancellationToken)
    {
        var reminderId = CreateReminderId(claimedReminder.TaskId, claimedReminder.Deadline);
        var reminderEvent = new TaskDeadlineReminderEvent(
            reminderId,
            claimedReminder.TaskId,
            claimedReminder.OwnerId,
            claimedReminder.TaskTitle,
            claimedReminder.Deadline,
            DateTimeOffset.UtcNow);

        try
        {
            await reminderPublisher.PublishAsync(reminderEvent, cancellationToken);
            var markedSent = await reminderRepository.MarkSentAsync(
                claimedReminder.TaskId,
                claimedReminder.LeaseId,
                cancellationToken);

            if (!markedSent)
            {
                DeadlineReminderLog.LeaseLost(logger, reminderId);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            DeadlineReminderLog.PublishFailed(logger, exception, reminderId);
        }
    }

    private static Guid CreateReminderId(Guid taskId, DateTimeOffset deadline)
    {
        var reminderKey = Encoding.UTF8.GetBytes($"{taskId:N}:{deadline.UtcDateTime.Ticks}");
        var hash = SHA256.HashData(reminderKey);
        return new Guid(hash.AsSpan(0, 16));
    }
}
