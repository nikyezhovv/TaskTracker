using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("tasks");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .HasColumnName("id");

            entity.Property(t => t.OwnerId)
                .HasColumnName("owner_id");

            entity.Property(t => t.Title)
                .HasColumnName("title")
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(t => t.Description)
                .HasColumnName("description");

            entity.Property(t => t.Deadline)
                .HasColumnName("deadline");

            entity.Property(t => t.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(t => t.Priority)
                .HasColumnName("priority")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(t => t.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(t => t.UpdatedAt)
                .HasColumnName("updated_at");
        });
    }
}
