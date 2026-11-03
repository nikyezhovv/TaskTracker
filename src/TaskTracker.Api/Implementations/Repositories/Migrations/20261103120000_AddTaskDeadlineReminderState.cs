using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using TaskTracker.Api.Implementations.Repositories;

namespace TaskTracker.Api.Implementations.Repositories.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261103120000_AddTaskDeadlineReminderState")]
public sealed class AddTaskDeadlineReminderState : Migration
{
    private static readonly string[] ReminderIndexColumns = { "deadline", "id" };

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "deadline_reminder_sent_at",
            table: "tasks",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "deadline_reminder_lease_until",
            table: "tasks",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "deadline_reminder_lease_id",
            table: "tasks",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_tasks_deadline_reminder_due",
            table: "tasks",
            columns: ReminderIndexColumns,
            filter: "deadline IS NOT NULL AND status <> 'Done' AND deadline_reminder_sent_at IS NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "ix_tasks_deadline_reminder_due", table: "tasks");

        migrationBuilder.DropColumn(name: "deadline_reminder_lease_id", table: "tasks");
        migrationBuilder.DropColumn(name: "deadline_reminder_lease_until", table: "tasks");
        migrationBuilder.DropColumn(name: "deadline_reminder_sent_at", table: "tasks");
    }
}
