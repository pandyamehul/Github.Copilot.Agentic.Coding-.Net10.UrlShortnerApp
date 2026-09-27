using System.Net.Http.Json;
using UrlTrimmer.WebApp.Models;

namespace UrlTrimmer.WebApp.Services;

public sealed class UrlShortenerApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ShortUrlViewModel>> GetUrlsAsync(string clerkUserId, string sessionToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/urls");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", sessionToken);
        request.Headers.Add("X-Clerk-Session-Token", sessionToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<ShortUrlResponse>>(cancellationToken: cancellationToken)
            ?? [];

        return items.Select(Map).ToList();
    }

    public async Task<ShortUrlViewModel> CreateAsync(CreateShortUrlRequest request, string sessionToken, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/urls")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", sessionToken);
        httpRequest.Headers.Add("X-Clerk-Session-Token", sessionToken);
        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken: cancellationToken);
            var detail = error?.Errors?.Values.SelectMany(messages => messages).FirstOrDefault()
                ?? error?.Detail
                ?? error?.Message
                ?? response.ReasonPhrase
                ?? "The link could not be created.";

            throw new HttpRequestException(detail, null, response.StatusCode);
        }

        var created = await response.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return the created short URL.");

        return Map(created);
    }

    private ShortUrlViewModel Map(ShortUrlResponse response)
    {
        var shortenedUrl = new Uri(httpClient.BaseAddress!, $"u/{response.Code}").ToString();
        return new ShortUrlViewModel(response.Id, response.Code, response.OriginalUrl, shortenedUrl, response.ClerkUserId, response.CreatedAt, response.UpdatedAt);
    }

    private sealed record ApiErrorResponse(
        string? Detail,
        string? Message,
        Dictionary<string, string[]>? Errors);
}