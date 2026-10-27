using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracker.Api.Implementations.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tags_pkey", x => x.id);
                    table.CheckConstraint("ck_tags_name_nonempty", "length(btrim(name)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                    table.CheckConstraint("ck_users_email_nonempty", "length(btrim(email)) > 0");
                    table.CheckConstraint("ck_users_full_name_nonempty", "length(btrim(full_name)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "meetings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organizer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    start_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    location = table.Column<string>(type: "text", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("meetings_pkey", x => x.id);
                    table.CheckConstraint("ck_meetings_time_range", "end_time >= start_time");
                    table.CheckConstraint("ck_meetings_title_nonempty", "length(btrim(title)) > 0");
                    table.ForeignKey(
                        name: "fk_meetings_organizer",
                        column: x => x.organizer_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("refresh_tokens_pkey", x => x.id);
                    table.CheckConstraint("ck_refresh_tokens_expiry", "expires_at > created_at");
                    table.CheckConstraint("ck_refresh_tokens_hash_nonempty", "length(btrim(token_hash)) > 0");
                    table.ForeignKey(
                        name: "fk_refresh_tokens_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("tasks_pkey", x => x.id);
                    table.CheckConstraint("ck_tasks_priority", "priority IN ('Low', 'Medium', 'High')");
                    table.CheckConstraint("ck_tasks_status", "status IN ('Todo', 'InProgress', 'Done')");
                    table.CheckConstraint("ck_tasks_title_nonempty", "length(btrim(title)) > 0");
                    table.ForeignKey(
                        name: "fk_tasks_owner",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "meeting_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meeting_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("meeting_links_pkey", x => x.id);
                    table.CheckConstraint("ck_meeting_links_position", "position >= 0");
                    table.CheckConstraint("ck_meeting_links_url_nonempty", "length(btrim(url)) > 0");
                    table.ForeignKey(
                        name: "fk_meeting_links_meeting",
                        column: x => x.meeting_id,
                        principalTable: "meetings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meeting_participants",
                columns: table => new
                {
                    meeting_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    participant_location = table.Column<string>(type: "text", nullable: true),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meeting_participants", x => new { x.meeting_id, x.user_id });
                    table.CheckConstraint("ck_meeting_participant_status", "participant_status IS NULL OR participant_status IN ('online', 'offline')");
                    table.ForeignKey(
                        name: "fk_meeting_participants_meeting",
                        column: x => x.meeting_id,
                        principalTable: "meetings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_meeting_participants_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "meeting_tag_assignments",
                columns: table => new
                {
                    meeting_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meeting_tag_assignments", x => new { x.meeting_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_meeting_tag_assignments_meeting",
                        column: x => x.meeting_id,
                        principalTable: "meetings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_meeting_tag_assignments_tag",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_tag_assignments",
                columns: table => new
                {
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_tag_assignments", x => new { x.task_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_task_tag_assignments_tag",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_tag_assignments_task",
                        column: x => x.task_id,
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "uq_meeting_links_position",
                table: "meeting_links",
                columns: new[] { "meeting_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_meeting_participants_user_id",
                table: "meeting_participants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_meeting_tag_assignments_tag_id",
                table: "meeting_tag_assignments",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_meetings_active_start",
                table: "meetings",
                column: "start_time",
                filter: "cancelled_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_meetings_organizer_start",
                table: "meetings",
                columns: new[] { "organizer_id", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_meetings_start_end",
                table: "meetings",
                columns: new[] { "start_time", "end_time" });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_active_expiry",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at" },
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_tags_name",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_task_tag_assignments_tag_id",
                table: "task_tag_assignments",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_deadline",
                table: "tasks",
                column: "deadline",
                filter: "deadline IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_owner_created",
                table: "tasks",
                columns: new[] { "owner_id", "created_at", "id" },
                descending: new[] { false, true, false });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_owner_deadline",
                table: "tasks",
                columns: new[] { "owner_id", "deadline" });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_owner_priority",
                table: "tasks",
                columns: new[] { "owner_id", "priority" });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_owner_status",
                table: "tasks",
                columns: new[] { "owner_id", "status" });

            migrationBuilder.CreateIndex(
                name: "uq_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meeting_links");

            migrationBuilder.DropTable(
                name: "meeting_participants");

            migrationBuilder.DropTable(
                name: "meeting_tag_assignments");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "task_tag_assignments");

            migrationBuilder.DropTable(
                name: "meetings");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
