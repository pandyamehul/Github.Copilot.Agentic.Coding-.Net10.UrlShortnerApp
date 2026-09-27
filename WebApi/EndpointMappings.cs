using Microsoft.EntityFrameworkCore;
using UrlTrimmer.WebApi.Contracts;
using UrlTrimmer.WebApi.Data;
using UrlTrimmer.WebApi.Models;
using UrlTrimmer.WebApi.Services;

namespace UrlTrimmer.WebApi;

public static class EndpointMappings
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
        MapUrlEndpoints(app);
    }

    private static void MapUrlEndpoints(WebApplication app)
    {
        app.MapGet("/api/urls", GetUrlsAsync).RequireAuthorization();
        app.MapPost("/api/urls", CreateUrlAsync).RequireAuthorization();
        app.MapGet("/api/urls/{code}", GetUrlAsync).RequireAuthorization();
        app.MapGet("/u/{code}", RedirectToUrlAsync);
    }

    private static async Task<IResult> GetUrlsAsync(
        HttpContext httpContext,
        UrlShortenerDbContext db,
        CancellationToken cancellationToken)
    {
        var clerkUserId = GetClerkUserId(httpContext);
        if (clerkUserId is null)
        {
            return Results.Unauthorized();
        }

        var items = await db.ShortUrls
            .Where(item => item.ClerkUserId == clerkUserId)
            .Select(item => item.ToResponse())
            .ToListAsync(cancellationToken);

        return Results.Ok(items.OrderByDescending(item => item.CreatedAt));
    }

    private static async Task<IResult> CreateUrlAsync(
        CreateShortUrlRequest request,
        HttpContext httpContext,
        UrlShortenerDbContext db,
        UrlCodeGenerator codeGenerator,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var originalUri) ||
            (originalUri.Scheme != Uri.UriSchemeHttp && originalUri.Scheme != Uri.UriSchemeHttps))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.OriginalUrl)] = ["A valid absolute http or https URL is required."]
            });
        }

        var clerkUserId = GetClerkUserId(httpContext);
        if (clerkUserId is null)
        {
            return Results.Unauthorized();
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
            ClerkUserId = clerkUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.ShortUrls.Add(shortUrl);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/urls/{code}", shortUrl.ToResponse());
    }

    private static async Task<IResult> GetUrlAsync(
        string code,
        HttpContext httpContext,
        UrlShortenerDbContext db,
        CancellationToken cancellationToken)
    {
        var clerkUserId = GetClerkUserId(httpContext);
        if (clerkUserId is null)
        {
            return Results.Unauthorized();
        }

        var shortUrl = await db.ShortUrls.FirstOrDefaultAsync(
            item => item.Code == code && item.ClerkUserId == clerkUserId,
            cancellationToken);

        return shortUrl is null
            ? Results.NotFound()
            : Results.Ok(shortUrl.ToResponse());
    }

    private static async Task<IResult> RedirectToUrlAsync(
        string code,
        UrlShortenerDbContext db,
        CancellationToken cancellationToken)
    {
        var shortUrl = await db.ShortUrls.FirstOrDefaultAsync(item => item.Code == code, cancellationToken);

        return shortUrl is null
            ? Results.NotFound()
            : Results.Redirect(shortUrl.OriginalUrl);
    }

    private static string? GetClerkUserId(HttpContext httpContext) =>
        httpContext.User.FindFirst("sub")?.Value is { Length: > 0 } userId ? userId : null;
}