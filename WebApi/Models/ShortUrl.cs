using UrlTrimmer.WebApi.Contracts;

namespace UrlTrimmer.WebApi.Models;

public sealed class ShortUrl
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string OriginalUrl { get; set; } = string.Empty;

    public string ClerkUserId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ShortUrlResponse ToResponse() => new(Id, Code, OriginalUrl, ClerkUserId, CreatedAt, UpdatedAt);
}