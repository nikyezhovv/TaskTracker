using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskTracker.Api.Implementations.Model;
using TaskTracker.Api.Implementations.Services;

namespace TaskTracker.Api.Implementations.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public sealed class TaskController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<TaskBatchModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskBatchModel>> GetTasks([FromQuery] TaskBatchRequest taskBatchRequest, CancellationToken cancellationToken)
    {
        if (taskBatchRequest.Page < 1 || taskBatchRequest.PageSize is < 1 or > 100)
        {
            return BadRequest("Invalid pagination parameters.");
        }

        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized();
        }

        return await taskService.GetBatchAsync(ownerId, taskBatchRequest, cancellationToken);
    }

    [HttpGet("{taskId:guid}")]
    [ProducesResponseType<TaskModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskModel>> GetTask(Guid taskId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized();
        }

        var taskModel = await taskService.GetByIdAsync(ownerId, taskId, cancellationToken);
        if (taskModel is null)
        {
            return NotFound();
        }

        return taskModel;
    }

    [HttpPost]
    [ProducesResponseType<TaskModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskModel>> CreateTask([FromBody] TaskCreateDtoRequest taskCreateDtoRequest, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized();
        }

        var taskModel = await taskService.CreateAsync(ownerId, taskCreateDtoRequest, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { taskId = taskModel.TaskId }, taskModel);
    }

    [HttpPut("{taskId:guid}")]
    [ProducesResponseType<TaskModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskModel>> UpdateTask(Guid taskId, [FromBody] TaskUpdateDtoRequest taskUpdateDtoRequest, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized();
        }

        var taskModel = await taskService.UpdateAsync(ownerId, taskId, taskUpdateDtoRequest, cancellationToken);
        if (taskModel is null)
        {
            return NotFound();
        }

        return taskModel;
    }

    [HttpDelete("{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteTask(Guid taskId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized();
        }

        var deleted = await taskService.DeleteAsync(ownerId, taskId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private bool TryGetOwnerId(out Guid ownerId)
    {
        var ownerClaimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(ownerClaimValue, out ownerId);
    }
}
