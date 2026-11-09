using Microsoft.EntityFrameworkCore;

using TaskTracker.Persistence.Entities;

namespace TaskTracker.Persistence;

public sealed class TaskReminderDbContext(DbContextOptions<TaskReminderDbContext> options) : DbContext(options)
{
    public DbSet<TaskReminderEntity> Tasks => Set<TaskReminderEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskReminderEntity>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Id).HasColumnName("id");
            entity.Property(task => task.OwnerId).HasColumnName("owner_id");
            entity.Property(task => task.Title).HasColumnName("title");
            entity.Property(task => task.Deadline).HasColumnName("deadline");
            entity.Property(task => task.Status).HasColumnName("status");
            entity.Property(task => task.DeadlineReminderSentAt).HasColumnName("deadline_reminder_sent_at");
            entity.Property(task => task.DeadlineReminderLeaseUntil).HasColumnName("deadline_reminder_lease_until");
            entity.Property(task => task.DeadlineReminderLeaseId).HasColumnName("deadline_reminder_lease_id");
            entity.HasIndex(task => new { task.Deadline, task.Id })
                .HasDatabaseName("ix_tasks_deadline_reminder_due")
                .HasFilter("deadline IS NOT NULL AND status <> 'Done' AND deadline_reminder_sent_at IS NULL");
        });
    }
}
