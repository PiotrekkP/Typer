using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Typer.Application.Integrations.ApiFootball;

namespace Typer.Infrastructure.Integrations.ApiFootball;

public sealed class ApiFootballClient : IApiFootballClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiFootballClient> _logger;

    public ApiFootballClient(HttpClient httpClient, ILogger<ApiFootballClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ApiFootballOddsLiveSnapshot>> GetOddsLiveFeedAsync(
        int? leagueId = null,
        CancellationToken cancellationToken = default)
    {
        var url = leagueId.HasValue ? $"odds/live?league={leagueId.Value}" : "odds/live";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        LogRateLimit(response);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiFootballResponse<ApiFootballOddsLiveItem>>(
            JsonOptions,
            cancellationToken);

        if (payload?.Response is null || payload.Response.Count == 0)
            return Array.Empty<ApiFootballOddsLiveSnapshot>();

        var snapshots = new List<ApiFootballOddsLiveSnapshot>(payload.Response.Count);
        foreach (var item in payload.Response)
        {
            var snapshot = MapItem(item);
            if (snapshot is not null)
                snapshots.Add(snapshot);
        }

        return snapshots;
    }

    public async Task<ApiFootballOddsLiveSnapshot?> GetOddsLiveByFixtureAsync(
        int fixtureId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"odds/live?fixture={fixtureId}", cancellationToken);
        LogRateLimit(response);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiFootballResponse<ApiFootballOddsLiveItem>>(
            JsonOptions,
            cancellationToken);

        return MapItem(payload?.Response?.FirstOrDefault());
    }

    private static ApiFootballOddsLiveSnapshot? MapItem(ApiFootballOddsLiveItem? item)
    {
        if (item?.Fixture is null || item.Teams?.Home is null || item.Teams.Away is null)
            return null;

        return new ApiFootballOddsLiveSnapshot(
            item.Fixture.Id,
            item.Teams.Home.Id,
            item.Teams.Away.Id,
            item.Teams.Home.Goals ?? 0,
            item.Teams.Away.Goals ?? 0,
            item.Fixture.Status?.Elapsed,
            item.Fixture.Status?.Long ?? string.Empty,
            item.Status?.Finished ?? false,
            item.Status?.Stopped ?? false,
            item.Status?.Blocked ?? false);
    }

    private void LogRateLimit(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-ratelimit-requests-remaining", out var remainingValues)
            && response.Headers.TryGetValues("x-ratelimit-requests-limit", out var limitValues))
        {
            _logger.LogInformation(
                "API-Football quota: {Remaining}/{Limit} pozostało dziś.",
                remainingValues.FirstOrDefault(),
                limitValues.FirstOrDefault());
        }
    }
}
