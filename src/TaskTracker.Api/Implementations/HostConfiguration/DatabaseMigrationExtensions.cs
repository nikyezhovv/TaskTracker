using System.Globalization;

using Microsoft.EntityFrameworkCore;

using TaskTracker.Api.Implementations.Repositories;

namespace TaskTracker.Api.Implementations.HostConfiguration;

public static class DatabaseMigrationExtensions
{
    private const string InitialMigrationId = "20261027115841_InitialCreate";

    public static async Task ApplyDatabaseMigrationsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

        await BaselineLegacySchemaAsync(dbContext, cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private static async Task BaselineLegacySchemaAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                SELECT COUNT(*)
                FROM unnest(ARRAY[
                    'users', 'refresh_tokens', 'tasks', 'tags', 'task_tag_assignments',
                    'meetings', 'meeting_participants', 'meeting_tag_assignments', 'meeting_links'
                ]) AS required_table(table_name)
                WHERE to_regclass(format('public.%I', table_name)) IS NOT NULL;
                """;

            var existingTableCount = Convert.ToInt32(
                await command.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture);
            if (existingTableCount is 0 or 9)
            {
                if (existingTableCount == 9)
                {
                    await using var historyCommand = dbContext.Database.GetDbConnection().CreateCommand();
                    historyCommand.CommandText = """
                        CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                            "MigrationId" character varying(150) NOT NULL,
                            "ProductVersion" character varying(32) NOT NULL,
                            CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
                        );
                        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                        VALUES (@migrationId, '10.0.0')
                        ON CONFLICT ("MigrationId") DO NOTHING;
                        """;
                    var migrationIdParameter = historyCommand.CreateParameter();
                    migrationIdParameter.ParameterName = "migrationId";
                    migrationIdParameter.Value = InitialMigrationId;
                    historyCommand.Parameters.Add(migrationIdParameter);
                    await historyCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                return;
            }

            throw new InvalidOperationException(
                $"Database contains {existingTableCount} of 9 legacy TaskTracker tables. " +
                "Automatic baseline is only safe for an empty database or a complete schema.");
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }
}
