namespace UrlTrimmer.WebApi.Contracts;

public sealed record ShortUrlResponse(int Id, string Code, string OriginalUrl, string ClerkUserId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);