using UrlTrimmer.WebApi.Contracts;

namespace UrlTrimmer.WebApi.Models;

public sealed class ShortUrl
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string OriginalUrl { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }

    public int ClickCount { get; set; }

    public ShortUrlResponse ToResponse() => new(Id, Code, OriginalUrl, CreatedUtc, ClickCount);
}