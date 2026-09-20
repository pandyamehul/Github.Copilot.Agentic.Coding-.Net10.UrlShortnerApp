using Microsoft.EntityFrameworkCore;
using UrlTrimmer.WebApi.Contracts;
using UrlTrimmer.WebApi.Data;
using UrlTrimmer.WebApi.Models;
using UrlTrimmer.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5044")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("UrlShortenerDb"));
});

builder.Services.AddScoped<UrlCodeGenerator>();

var app = builder.Build();

app.UseCors("WebApp");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>();
    db.Database.EnsureCreated();

    var connection = db.Database.GetDbConnection();
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = "PRAGMA table_info(\"ShortUrls\")";
    using var reader = command.ExecuteReader();
    var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    while (reader.Read())
    {
        columns.Add(reader.GetString(1));
    }

    reader.Close();
    if (columns.Any(column => string.Equals(column, "OriginalUrl", StringComparison.Ordinal)))
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
        command.ExecuteNonQuery();
        columns = ["Id", "code", "original_url", "clerk_user_id", "created_at", "updated_at"];
    }

    var missingColumns = new Dictionary<string, string>
    {
        ["original_url"] = "TEXT NOT NULL DEFAULT ''",
        ["clerk_user_id"] = "TEXT NOT NULL DEFAULT 'anonymous'",
        ["created_at"] = "TEXT NOT NULL DEFAULT '1970-01-01T00:00:00+00:00'",
        ["updated_at"] = "TEXT NOT NULL DEFAULT '1970-01-01T00:00:00+00:00'"
    };

    foreach (var missingColumn in missingColumns.Where(item => !columns.Contains(item.Key)))
    {
        command.CommandText = $"ALTER TABLE \"ShortUrls\" ADD COLUMN {missingColumn.Key} {missingColumn.Value}";
        command.ExecuteNonQuery();
    }
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/urls", async (string? clerkUserId, UrlShortenerDbContext db, CancellationToken cancellationToken) =>
{
    var query = db.ShortUrls.AsQueryable();

    if (!string.IsNullOrWhiteSpace(clerkUserId))
    {
        query = query.Where(item => item.ClerkUserId == clerkUserId);
    }

    var items = await query
        .Select(item => item.ToResponse())
        .ToListAsync(cancellationToken);

    return Results.Ok(items.OrderByDescending(item => item.CreatedAt));
});

app.MapPost("/api/urls", async (
    CreateShortUrlRequest request,
    UrlShortenerDbContext db,
    UrlCodeGenerator codeGenerator,
    CancellationToken cancellationToken) =>
{
    if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var originalUri) ||
        (originalUri.Scheme != Uri.UriSchemeHttp && originalUri.Scheme != Uri.UriSchemeHttps))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.OriginalUrl)] = ["A valid absolute http or https URL is required."]
        });
    }

    if (string.IsNullOrWhiteSpace(request.ClerkUserId))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.ClerkUserId)] = ["A signed-in Clerk user id is required."]
        });
    }

    var code = string.IsNullOrWhiteSpace(request.CustomCode)
        ? codeGenerator.GenerateCode()
        : request.CustomCode.Trim();

    if (code.Length is < 3 or > 32 || code.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_'))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.CustomCode)] = ["Custom code must be 3 to 32 characters and contain only letters, numbers, '-' or '_'."]
        });
    }

    if (await db.ShortUrls.AnyAsync(item => item.Code == code, cancellationToken))
    {
        return Results.Conflict(new { message = "That short code is already in use." });
    }

    var shortUrl = new ShortUrl
    {
        Code = code,
        OriginalUrl = originalUri.ToString(),
        ClerkUserId = request.ClerkUserId,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    db.ShortUrls.Add(shortUrl);
    await db.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/urls/{code}", shortUrl.ToResponse());
});

app.MapGet("/api/urls/{code}", async (string code, UrlShortenerDbContext db, CancellationToken cancellationToken) =>
{
    var shortUrl = await db.ShortUrls.FirstOrDefaultAsync(item => item.Code == code, cancellationToken);

    return shortUrl is null
        ? Results.NotFound()
        : Results.Ok(shortUrl.ToResponse());
});

app.MapGet("/u/{code}", async (string code, UrlShortenerDbContext db, CancellationToken cancellationToken) =>
{
    var shortUrl = await db.ShortUrls.FirstOrDefaultAsync(item => item.Code == code, cancellationToken);

    if (shortUrl is null)
    {
        return Results.NotFound();
    }

    // No click tracking per schema: do not increment counters here.

    return Results.Redirect(shortUrl.OriginalUrl);
});

app.Run();