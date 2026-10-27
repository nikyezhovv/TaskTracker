using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Implementations.Model;
using TaskTracker.Api.Implementations.Repositories.Entities;

namespace TaskTracker.Api.Implementations.Repositories;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<TaskTagAssignmentEntity> TaskTagAssignments => Set<TaskTagAssignmentEntity>();
    public DbSet<MeetingEntity> Meetings => Set<MeetingEntity>();
    public DbSet<MeetingParticipantEntity> MeetingParticipants => Set<MeetingParticipantEntity>();
    public DbSet<MeetingTagAssignmentEntity> MeetingTagAssignments => Set<MeetingTagAssignmentEntity>();
    public DbSet<MeetingLinkEntity> MeetingLinks => Set<MeetingLinkEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.ToTable("users", t =>
            {
                t.HasCheckConstraint("ck_users_email_nonempty", "length(btrim(email)) > 0");
                t.HasCheckConstraint("ck_users_full_name_nonempty", "length(btrim(full_name)) > 0");
            });
            e.HasKey(x => x.Id).HasName("users_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
            e.Property(x => x.Username).HasColumnName("username").HasMaxLength(64);
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("uq_users_email");
            e.HasIndex(x => x.Username).IsUnique().HasDatabaseName("uq_users_username");
        });

        modelBuilder.Entity<RefreshTokenEntity>(e =>
        {
            e.ToTable("refresh_tokens", t =>
            {
                t.HasCheckConstraint("ck_refresh_tokens_hash_nonempty", "length(btrim(token_hash)) > 0");
                t.HasCheckConstraint("ck_refresh_tokens_expiry", "expires_at > created_at");
            });
            e.HasKey(x => x.Id).HasName("refresh_tokens_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired();
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasIndex(x => x.TokenHash).IsUnique().HasDatabaseName("uq_refresh_tokens_token_hash");
            e.HasIndex(x => x.UserId).HasDatabaseName("ix_refresh_tokens_user_id");
            e.HasIndex(x => new { x.UserId, x.ExpiresAt }).HasDatabaseName("ix_refresh_tokens_active_expiry").HasFilter("revoked_at IS NULL");
            e.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("fk_refresh_tokens_user");
        });

        modelBuilder.Entity<TaskEntity>(e =>
        {
            e.ToTable("tasks", t =>
            {
                t.HasCheckConstraint("ck_tasks_title_nonempty", "length(btrim(title)) > 0");
                t.HasCheckConstraint("ck_tasks_status", "status IN ('Todo', 'InProgress', 'Done')");
                t.HasCheckConstraint("ck_tasks_priority", "priority IN ('Low', 'Medium', 'High')");
            });
            e.HasKey(x => x.Id).HasName("tasks_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.OwnerId).HasColumnName("owner_id");
            e.Property(x => x.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Deadline).HasColumnName("deadline");
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Priority).HasColumnName("priority").HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasIndex(x => new { x.OwnerId, x.CreatedAt, x.Id }).HasDatabaseName("ix_tasks_owner_created").IsDescending(false, true, false);
            e.HasIndex(x => new { x.OwnerId, x.Status }).HasDatabaseName("ix_tasks_owner_status");
            e.HasIndex(x => new { x.OwnerId, x.Priority }).HasDatabaseName("ix_tasks_owner_priority");
            e.HasIndex(x => new { x.OwnerId, x.Deadline }).HasDatabaseName("ix_tasks_owner_deadline");
            e.HasIndex(x => x.Deadline).HasDatabaseName("ix_tasks_deadline").HasFilter("deadline IS NOT NULL");
            e.HasOne(x => x.Owner).WithMany(x => x.Tasks).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("fk_tasks_owner");
        });

        modelBuilder.Entity<TagEntity>(e =>
        {
            e.ToTable("tags", t => t.HasCheckConstraint("ck_tags_name_nonempty", "length(btrim(name)) > 0"));
            e.HasKey(x => x.Id).HasName("tags_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("uq_tags_name");
        });

        modelBuilder.Entity<TaskTagAssignmentEntity>(e =>
        {
            e.ToTable("task_tag_assignments");
            e.HasKey(x => new { x.TaskId, x.TagId }).HasName("pk_task_tag_assignments");
            e.Property(x => x.TaskId).HasColumnName("task_id");
            e.Property(x => x.TagId).HasColumnName("tag_id");
            e.HasIndex(x => x.TagId).HasDatabaseName("ix_task_tag_assignments_tag_id");
            e.HasOne(x => x.Task).WithMany(x => x.TagAssignments).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_task_tag_assignments_task");
            e.HasOne(x => x.Tag).WithMany(x => x.TaskAssignments).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_task_tag_assignments_tag");
        });

        modelBuilder.Entity<MeetingEntity>(e =>
        {
            e.ToTable("meetings", t =>
            {
                t.HasCheckConstraint("ck_meetings_title_nonempty", "length(btrim(title)) > 0");
                t.HasCheckConstraint("ck_meetings_time_range", "end_time >= start_time");
            });
            e.HasKey(x => x.Id).HasName("meetings_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.OrganizerId).HasColumnName("organizer_id");
            e.Property(x => x.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.StartTime).HasColumnName("start_time");
            e.Property(x => x.EndTime).HasColumnName("end_time");
            e.Property(x => x.Location).HasColumnName("location");
            e.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasIndex(x => new { x.OrganizerId, x.StartTime }).HasDatabaseName("ix_meetings_organizer_start");
            e.HasIndex(x => new { x.StartTime, x.EndTime }).HasDatabaseName("ix_meetings_start_end");
            e.HasIndex(x => x.StartTime).HasDatabaseName("ix_meetings_active_start").HasFilter("cancelled_at IS NULL");
            e.HasOne(x => x.Organizer).WithMany(x => x.OrganizedMeetings).HasForeignKey(x => x.OrganizerId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("fk_meetings_organizer");
        });

        modelBuilder.Entity<MeetingParticipantEntity>(e =>
        {
            e.ToTable("meeting_participants", t => t.HasCheckConstraint("ck_meeting_participant_status", "participant_status IS NULL OR participant_status IN ('online', 'offline')"));
            e.HasKey(x => new { x.MeetingId, x.UserId }).HasName("pk_meeting_participants");
            e.Property(x => x.MeetingId).HasColumnName("meeting_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.ParticipantStatus).HasColumnName("participant_status").HasMaxLength(20);
            e.Property(x => x.ParticipantLocation).HasColumnName("participant_location");
            e.Property(x => x.AddedAt).HasColumnName("added_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasIndex(x => x.UserId).HasDatabaseName("ix_meeting_participants_user_id");
            e.HasOne(x => x.Meeting).WithMany(x => x.Participants).HasForeignKey(x => x.MeetingId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_meeting_participants_meeting");
            e.HasOne(x => x.User).WithMany(x => x.MeetingParticipations).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("fk_meeting_participants_user");
        });

        modelBuilder.Entity<MeetingTagAssignmentEntity>(e =>
        {
            e.ToTable("meeting_tag_assignments");
            e.HasKey(x => new { x.MeetingId, x.TagId }).HasName("pk_meeting_tag_assignments");
            e.Property(x => x.MeetingId).HasColumnName("meeting_id");
            e.Property(x => x.TagId).HasColumnName("tag_id");
            e.HasIndex(x => x.TagId).HasDatabaseName("ix_meeting_tag_assignments_tag_id");
            e.HasOne(x => x.Meeting).WithMany(x => x.TagAssignments).HasForeignKey(x => x.MeetingId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_meeting_tag_assignments_meeting");
            e.HasOne(x => x.Tag).WithMany(x => x.MeetingAssignments).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_meeting_tag_assignments_tag");
        });

        modelBuilder.Entity<MeetingLinkEntity>(e =>
        {
            e.ToTable("meeting_links", t =>
            {
                t.HasCheckConstraint("ck_meeting_links_url_nonempty", "length(btrim(url)) > 0");
                t.HasCheckConstraint("ck_meeting_links_position", "position >= 0");
            });
            e.HasKey(x => x.Id).HasName("meeting_links_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.MeetingId).HasColumnName("meeting_id");
            e.Property(x => x.Url).HasColumnName("url").IsRequired();
            e.Property(x => x.Position).HasColumnName("position");
            e.HasIndex(x => new { x.MeetingId, x.Position }).IsUnique().HasDatabaseName("uq_meeting_links_position");
            e.HasOne(x => x.Meeting).WithMany(x => x.Links).HasForeignKey(x => x.MeetingId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_meeting_links_meeting");
        });
    }
}
