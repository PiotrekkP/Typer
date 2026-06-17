using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Typer.Application.Integrations.FootballData;

namespace Typer.Infrastructure.Integrations.FootballData;

public sealed class FootballDataClient : IFootballDataClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<FootballDataClient> _logger;

    public FootballDataClient(HttpClient httpClient, ILogger<FootballDataClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FootballDataLiveMatchSnapshot>> GetLiveMatchesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("matches?status=LIVE", cancellationToken);
        LogRateLimit(response);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<FootballDataMatchesResponse>(
            JsonOptions,
            cancellationToken);

        return MapMatches(payload?.Matches);
    }

    public async Task<FootballDataLiveMatchSnapshot?> GetMatchAsync(
        int matchId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"matches/{matchId}", cancellationToken);
        LogRateLimit(response);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<FootballDataMatchItem>(
            JsonOptions,
            cancellationToken);

        return MapMatch(payload);
    }

    private static IReadOnlyList<FootballDataLiveMatchSnapshot> MapMatches(
        IReadOnlyList<FootballDataMatchItem>? items)
    {
        if (items is null || items.Count == 0)
            return Array.Empty<FootballDataLiveMatchSnapshot>();

        var snapshots = new List<FootballDataLiveMatchSnapshot>(items.Count);
        foreach (var item in items)
        {
            var snapshot = MapMatch(item);
            if (snapshot is not null)
                snapshots.Add(snapshot);
        }

        return snapshots;
    }

    private static FootballDataLiveMatchSnapshot? MapMatch(FootballDataMatchItem? item)
    {
        if (item?.HomeTeam is null || item.AwayTeam is null || string.IsNullOrWhiteSpace(item.Status))
            return null;

        return new FootballDataLiveMatchSnapshot(
            item.Id,
            item.HomeTeam.Id,
            item.AwayTeam.Id,
            item.HomeTeam.Tla ?? string.Empty,
            item.AwayTeam.Tla ?? string.Empty,
            item.HomeTeam.Name ?? string.Empty,
            item.AwayTeam.Name ?? string.Empty,
            item.Score?.FullTime?.Home,
            item.Score?.FullTime?.Away,
            item.Status,
            item.UtcDate,
            item.LastUpdated);
    }

    private void LogRateLimit(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("X-Requests-Available-Minute", out var minuteValues))
        {
            _logger.LogInformation(
                "football-data.org: {Remaining} requestów/min pozostało.",
                minuteValues.FirstOrDefault());
        }
    }
}
