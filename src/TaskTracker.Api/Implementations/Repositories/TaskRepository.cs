using Microsoft.EntityFrameworkCore;

using TaskTracker.Api.Implementations.Model;
using TaskTracker.Api.Implementations.Repositories.Entities;

namespace TaskTracker.Api.Implementations.Repositories;

public sealed class TaskRepository(AppDbContext dbContext) : ITaskRepository
{
    public async Task<TaskBatchModel> GetBatchAsync(Guid ownerId, TaskBatchRequest taskBatchRequest, CancellationToken cancellationToken)
    {
        var tasks = dbContext.Tasks.AsNoTracking().Where(task => task.OwnerId == ownerId);

        if (taskBatchRequest.Status is { } taskStatus)
        {
            tasks = tasks.Where(task => task.Status == taskStatus);
        }

        if (taskBatchRequest.Priority is { } taskPriority)
        {
            tasks = tasks.Where(task => task.Priority == taskPriority);
        }

        if (taskBatchRequest.Date is { } date)
        {
            var start = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            var end = start.AddDays(1);
            tasks = tasks.Where(task => task.Deadline >= start && task.Deadline < end);
        }

        var tagNames = ParseTags(taskBatchRequest.Tags);
        if (tagNames.Length > 0)
        {
            tasks = tasks.Where(task => task.TagAssignments.Any(assignment => tagNames.Contains(assignment.Tag.Name)));
        }

        var totalItems = await tasks.CountAsync(cancellationToken);
        var entities = await tasks
            .Include(task => task.TagAssignments).ThenInclude(assignment => assignment.Tag)
            .OrderByDescending(task => task.CreatedAt).ThenBy(task => task.Id)
            .Skip((taskBatchRequest.Page - 1) * taskBatchRequest.PageSize).Take(taskBatchRequest.PageSize)
            .ToListAsync(cancellationToken);

        return new TaskBatchModel(
            entities.Select(Map).ToArray(),
            new TaskPaginationModel(taskBatchRequest.Page, taskBatchRequest.PageSize, totalItems,
                (int)Math.Ceiling((double)totalItems / taskBatchRequest.PageSize)));
    }

    public async Task<TaskModel?> GetByIdAsync(Guid ownerId, Guid taskId, CancellationToken cancellationToken)
    {
        var taskEntity = await dbContext.Tasks.AsNoTracking()
            .Include(task => task.TagAssignments).ThenInclude(assignment => assignment.Tag)
            .FirstOrDefaultAsync(task => task.Id == taskId && task.OwnerId == ownerId, cancellationToken);
        return taskEntity is null ? null : Map(taskEntity);
    }

    public async Task<TaskModel> CreateAsync(Guid ownerId, TaskCreateDtoRequest taskCreateDtoRequest, CancellationToken cancellationToken)
    {
        var taskEntity = new TaskEntity
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = taskCreateDtoRequest.Title.Trim(),
            Description = taskCreateDtoRequest.Description,
            Deadline = taskCreateDtoRequest.Deadline?.ToUniversalTime(),
            Status = TaskState.Todo,
            Priority = taskCreateDtoRequest.Priority,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var tags = await GetOrCreateTagsAsync(taskCreateDtoRequest.Tags, cancellationToken);
        taskEntity.TagAssignments = tags.Select(tag => new TaskTagAssignmentEntity { Task = taskEntity, Tag = tag }).ToList();
        dbContext.Tasks.Add(taskEntity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(taskEntity);
    }

    public async Task<TaskModel?> UpdateAsync(Guid ownerId, Guid taskId, TaskUpdateDtoRequest taskUpdateDtoRequest, CancellationToken cancellationToken)
    {
        var taskEntity = await dbContext.Tasks
            .Include(task => task.TagAssignments).ThenInclude(assignment => assignment.Tag)
            .FirstOrDefaultAsync(task => task.Id == taskId && task.OwnerId == ownerId, cancellationToken);
        if (taskEntity is null)
        {
            return null;
        }

        var updatedDeadline = taskUpdateDtoRequest.Deadline?.ToUniversalTime();
        if (taskEntity.Deadline != updatedDeadline)
        {
            taskEntity.DeadlineReminderSentAt = null;
            taskEntity.DeadlineReminderLeaseUntil = null;
            taskEntity.DeadlineReminderLeaseId = null;
        }

        taskEntity.Title = taskUpdateDtoRequest.Title.Trim();
        taskEntity.Description = taskUpdateDtoRequest.Description;
        taskEntity.Deadline = updatedDeadline;
        taskEntity.Status = taskUpdateDtoRequest.Status;
        taskEntity.Priority = taskUpdateDtoRequest.Priority;
        taskEntity.UpdatedAt = DateTimeOffset.UtcNow;
        var desiredTags = await GetOrCreateTagsAsync(taskUpdateDtoRequest.Tags, cancellationToken);
        var desiredIds = desiredTags.Select(tag => tag.Id).ToHashSet();
        foreach (var assignment in taskEntity.TagAssignments.Where(assignment => !desiredIds.Contains(assignment.TagId)).ToArray())
        {
            taskEntity.TagAssignments.Remove(assignment);
            dbContext.TaskTagAssignments.Remove(assignment);
        }

        var currentIds = taskEntity.TagAssignments.Select(assignment => assignment.TagId).ToHashSet();
        foreach (var tag in desiredTags.Where(tag => !currentIds.Contains(tag.Id)))
        {
            taskEntity.TagAssignments.Add(new TaskTagAssignmentEntity { Task = taskEntity, Tag = tag });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(taskEntity);
    }

    public async Task<bool> DeleteAsync(Guid ownerId, Guid taskId, CancellationToken cancellationToken)
    {
        var taskEntity = await dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == taskId && task.OwnerId == ownerId, cancellationToken);
        if (taskEntity is null)
        {
            return false;
        }

        dbContext.Tasks.Remove(taskEntity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<List<TagEntity>> GetOrCreateTagsAsync(
        IReadOnlyList<string>? requestedTagNames, CancellationToken cancellationToken)
    {
        var tagNames = (requestedTagNames ?? Array.Empty<string>()).Select(tagName => tagName.Trim())
            .Where(tagName => tagName.Length > 0).Distinct(StringComparer.Ordinal).ToArray();
        if (tagNames.Length == 0)
        {
            return new List<TagEntity>();
        }

        var tags = await dbContext.Tags.Where(tag => tagNames.Contains(tag.Name)).ToListAsync(cancellationToken);
        var existingNames = tags.Select(tag => tag.Name).ToHashSet(StringComparer.Ordinal);
        foreach (var tagName in tagNames.Where(tagName => !existingNames.Contains(tagName)))
        {
            var tagEntity = new TagEntity { Id = Guid.NewGuid(), Name = tagName };
            dbContext.Tags.Add(tagEntity);
            tags.Add(tagEntity);
        }

        return tags;
    }

    private static TaskModel Map(TaskEntity taskEntity) => new(
        taskEntity.Id, taskEntity.Title, taskEntity.Description, taskEntity.Deadline, taskEntity.Status, taskEntity.Priority,
        taskEntity.TagAssignments.Select(assignment => assignment.Tag.Name).Order().ToArray(),
        taskEntity.CreatedAt, taskEntity.UpdatedAt);

    private static string[] ParseTags(string? tagsFilter) => string.IsNullOrWhiteSpace(tagsFilter)
        ? Array.Empty<string>()
        : tagsFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.Ordinal).ToArray();
}
