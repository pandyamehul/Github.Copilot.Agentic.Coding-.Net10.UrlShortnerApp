using System.Net.Http.Json;
using UrlTrimmer.WebApp.Models;

namespace UrlTrimmer.WebApp.Services;

public sealed class UrlShortenerApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ShortUrlViewModel>> GetUrlsAsync(CancellationToken cancellationToken = default)
    {
        var items = await httpClient.GetFromJsonAsync<List<ShortUrlResponse>>("api/urls", cancellationToken)
            ?? [];

        return items.Select(Map).ToList();
    }

    public async Task<ShortUrlViewModel> CreateAsync(CreateShortUrlRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/urls", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return the created short URL.");

        return Map(created);
    }

    private ShortUrlViewModel Map(ShortUrlResponse response)
    {
        var shortenedUrl = new Uri(httpClient.BaseAddress!, $"u/{response.Code}").ToString();
        return new ShortUrlViewModel(response.Id, response.Code, response.OriginalUrl, shortenedUrl, response.CreatedUtc, response.ClickCount);
    }
}