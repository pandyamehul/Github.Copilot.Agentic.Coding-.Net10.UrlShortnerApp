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
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/urls", async (UrlShortenerDbContext db, CancellationToken cancellationToken) =>
{
    var items = await db.ShortUrls
        .OrderByDescending(item => item.CreatedUtc)
        .Select(item => item.ToResponse())
        .ToListAsync(cancellationToken);

    return Results.Ok(items);
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
        CreatedUtc = DateTime.UtcNow
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

    shortUrl.ClickCount += 1;
    await db.SaveChangesAsync(cancellationToken);

    return Results.Redirect(shortUrl.OriginalUrl);
});

app.Run();