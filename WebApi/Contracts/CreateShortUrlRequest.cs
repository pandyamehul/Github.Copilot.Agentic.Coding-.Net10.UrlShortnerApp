namespace UrlTrimmer.WebApi.Contracts;

public sealed record CreateShortUrlRequest(string OriginalUrl, string? CustomCode);