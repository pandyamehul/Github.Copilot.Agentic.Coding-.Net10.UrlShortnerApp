namespace UrlTrimmer.WebApp.Models;

public sealed record CreateShortUrlRequest(string OriginalUrl, string? CustomCode);