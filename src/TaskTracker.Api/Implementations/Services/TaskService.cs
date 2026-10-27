using TaskTracker.Api.Implementations.Model;
using TaskTracker.Api.Implementations.Repositories;

namespace TaskTracker.Api.Implementations.Services;

public sealed class TaskService(ITaskRepository taskRepository) : ITaskService
{
    public Task<TaskBatchModel> GetBatchAsync(Guid ownerId, TaskBatchRequest taskBatchRequest, CancellationToken cancellationToken) =>
        taskRepository.GetBatchAsync(ownerId, taskBatchRequest, cancellationToken);

    public Task<TaskModel?> GetByIdAsync(Guid ownerId, Guid taskId, CancellationToken cancellationToken) =>
        taskRepository.GetByIdAsync(ownerId, taskId, cancellationToken);

    public Task<TaskModel> CreateAsync(Guid ownerId, TaskCreateDtoRequest taskCreateDtoRequest, CancellationToken cancellationToken) =>
        taskRepository.CreateAsync(ownerId, taskCreateDtoRequest, cancellationToken);

    public Task<TaskModel?> UpdateAsync(Guid ownerId, Guid taskId, TaskUpdateDtoRequest taskUpdateDtoRequest, CancellationToken cancellationToken)
    {
        return taskRepository.UpdateAsync(ownerId, taskId, taskUpdateDtoRequest, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid ownerId, Guid taskId, CancellationToken cancellationToken) =>
        taskRepository.DeleteAsync(ownerId, taskId, cancellationToken);
}
