using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.DTOs;

namespace TaskTracker.Api.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks")
            .WithTags("Tasks");

        group.MapGet("/", GetTasks);
        group.MapGet("/{id:guid}", GetTaskById);

        return app;
    }

    private static async Task<IResult> GetTasks(
        AppDbContext db,
        int page = 1,
        int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return Results.BadRequest("Invalid pagination parameters.");

        var query = db.Tasks.AsNoTracking();

        var totalItems = await query.CountAsync();

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskResponse(
                t.Id,
                t.Title,
                t.Deadline,
                t.Status,
                t.Priority
            ))
            .ToListAsync();

        return Results.Ok(new
        {
            Items = tasks,
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(
                    (double)totalItems / pageSize)
            }
        });
    }

    private static async Task<IResult> GetTaskById(
        Guid id,
        AppDbContext db)
    {
        var task = await db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                TaskId = t.Id,
                t.Title,
                t.Description,
                t.Deadline,
                t.Status,
                t.Priority
            })
            .FirstOrDefaultAsync();

        return task is null
            ? Results.NotFound()
            : Results.Ok(task);
    }
}
