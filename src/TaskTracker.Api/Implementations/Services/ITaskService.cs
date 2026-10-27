using TaskTracker.Api.Implementations.Model;

namespace TaskTracker.Api.Implementations.Services;

public interface ITaskService
{
    Task<TaskBatchModel> GetBatchAsync(Guid ownerId, TaskBatchRequest taskBatchRequest, CancellationToken cancellationToken);
    Task<TaskModel?> GetByIdAsync(Guid ownerId, Guid taskId, CancellationToken cancellationToken);
    Task<TaskModel> CreateAsync(Guid ownerId, TaskCreateDtoRequest taskCreateDtoRequest, CancellationToken cancellationToken);
    Task<TaskModel?> UpdateAsync(Guid ownerId, Guid taskId, TaskUpdateDtoRequest taskUpdateDtoRequest, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid ownerId, Guid taskId, CancellationToken cancellationToken);
}
