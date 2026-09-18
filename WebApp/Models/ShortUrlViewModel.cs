namespace UrlTrimmer.WebApp.Models;

public sealed record ShortUrlViewModel(int Id, string Code, string OriginalUrl, string ShortenedUrl, string ClerkUserId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);