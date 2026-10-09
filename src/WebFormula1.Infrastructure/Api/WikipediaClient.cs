using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WebFormula1.Infrastructure.Api;

public interface IWikipediaClient
{
    Task<string?> GetSummaryAsync(string articleUrl, CancellationToken ct);
}

public class WikipediaClient : IWikipediaClient
{
    private readonly HttpClient _http;

    public WikipediaClient(HttpClient http) => _http = http;

    public async Task<string?> GetSummaryAsync(string articleUrl, CancellationToken ct)
    {
        const string marker = "/wiki/";
        var index = articleUrl.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0) return null;

        var title = articleUrl[(index + marker.Length)..];
        using var response = await _http.GetAsync($"api/rest_v1/page/summary/{title}", ct);
        if (!response.IsSuccessStatusCode) return null;

        var summary = await response.Content.ReadFromJsonAsync<SummaryResponse>(cancellationToken: ct);
        return string.IsNullOrWhiteSpace(summary?.Extract) ? null : summary.Extract;
    }

    private class SummaryResponse
    {
        [JsonPropertyName("extract")]
        public string? Extract { get; set; }
    }
}
