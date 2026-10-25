namespace TaskTracker.Api.DTOs;

public record TaskResponse(
    Guid TaskId,
    string Title,
    DateTime? Deadline,
    string Status,
    string Priority
);
