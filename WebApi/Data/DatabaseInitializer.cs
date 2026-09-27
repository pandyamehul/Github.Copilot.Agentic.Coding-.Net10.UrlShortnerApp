using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace UrlTrimmer.WebApi.Data;

public static class DatabaseInitializer
{
    private static readonly IReadOnlyDictionary<string, string> MissingColumnSql =
        new Dictionary<string, string>
        {
            ["original_url"] = "ALTER TABLE \"ShortUrls\" ADD COLUMN original_url TEXT NOT NULL DEFAULT ''",
            ["clerk_user_id"] = "ALTER TABLE \"ShortUrls\" ADD COLUMN clerk_user_id TEXT NOT NULL DEFAULT 'anonymous'",
            ["created_at"] = "ALTER TABLE \"ShortUrls\" ADD COLUMN created_at TEXT NOT NULL DEFAULT '1970-01-01T00:00:00+00:00'",
            ["updated_at"] = "ALTER TABLE \"ShortUrls\" ADD COLUMN updated_at TEXT NOT NULL DEFAULT '1970-01-01T00:00:00+00:00'"
        };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>();
        await db.Database.EnsureCreatedAsync();

        await using var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(\"ShortUrls\")";
        var columns = await ReadColumnsAsync(command);

        if (columns.Contains("OriginalUrl"))
        {
            command.CommandText = """
                CREATE TABLE "ShortUrls_v2" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_ShortUrls" PRIMARY KEY AUTOINCREMENT,
                    "code" TEXT NOT NULL,
                    "original_url" TEXT NOT NULL,
                    "clerk_user_id" TEXT NOT NULL,
                    "created_at" TEXT NOT NULL,
                    "updated_at" TEXT NOT NULL
                );
                INSERT INTO "ShortUrls_v2" ("Id", "code", "original_url", "clerk_user_id", "created_at", "updated_at")
                SELECT "Id", "Code", COALESCE("original_url", "OriginalUrl", ''), COALESCE("clerk_user_id", 'anonymous'),
                       COALESCE("created_at", '1970-01-01T00:00:00+00:00'), COALESCE("updated_at", '1970-01-01T00:00:00+00:00')
                FROM "ShortUrls";
                DROP TABLE "ShortUrls";
                ALTER TABLE "ShortUrls_v2" RENAME TO "ShortUrls";
                CREATE UNIQUE INDEX "idx_short_urls_code" ON "ShortUrls" ("code");
                """;
            await command.ExecuteNonQueryAsync();
            columns = ["Id", "code", "original_url", "clerk_user_id", "created_at", "updated_at"];
        }

        foreach (var missingColumn in MissingColumnSql.Where(item => !columns.Contains(item.Key)))
        {
            command.CommandText = missingColumn.Value;
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task<HashSet<string>> ReadColumnsAsync(DbCommand command)
    {
        await using var reader = await command.ExecuteReaderAsync();
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }
}