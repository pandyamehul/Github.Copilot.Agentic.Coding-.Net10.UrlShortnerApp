namespace UrlTrimmer.WebApi.Contracts;

public sealed record ShortUrlResponse(int Id, string Code, string OriginalUrl, DateTime CreatedUtc, int ClickCount);