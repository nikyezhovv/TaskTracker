using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskTracker.Api.Implementations.Model;

public enum TaskState
{
    Todo,
    InProgress,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}

public sealed record TaskModel(
    Guid TaskId,
    string Title,
    string? Description,
    DateTimeOffset? Deadline,
    TaskState Status,
    TaskPriority Priority,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TaskBatchModel(
    IReadOnlyList<TaskModel> Items,
    TaskPaginationModel Pagination);

public sealed record TaskPaginationModel(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record TaskCreateDtoRequest(
    [param: Required, StringLength(300), RegularExpression(".*\\S.*")] string Title,
    string? Description,
    DateTimeOffset? Deadline,
    [property: JsonRequired] TaskPriority Priority,
    IReadOnlyList<string>? Tags);

public sealed record TaskUpdateDtoRequest(
    [param: Required, StringLength(300), RegularExpression(".*\\S.*")] string Title,
    string? Description,
    DateTimeOffset? Deadline,
    [property: JsonRequired] TaskState Status,
    [property: JsonRequired] TaskPriority Priority,
    IReadOnlyList<string>? Tags);

public sealed record TaskBatchRequest(
    int Page = 1,
    int PageSize = 20,
    DateOnly? Date = null,
    TaskState? Status = null,
    TaskPriority? Priority = null,
    string? Tags = null);
