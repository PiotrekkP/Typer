using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typer.Application.Integrations.ApiFootball;
using Typer.Infrastructure.Options;

namespace Typer.Infrastructure.Integrations.ApiFootball;

public sealed class ApiFootballClient : IApiFootballClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ApiFootballOptions _options;
    private readonly ILogger<ApiFootballClient> _logger;

    public ApiFootballClient(
        HttpClient httpClient,
        IOptions<ApiFootballOptions> options,
        ILogger<ApiFootballClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ApiFootballLiveFixture>> GetLiveFixturesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("fixtures?live=all", cancellationToken);
        LogRateLimit(response);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiFootballResponse<ApiFootballFixtureItem>>(
            JsonOptions,
            cancellationToken);

        if (payload?.Response is null || payload.Response.Count == 0)
            return Array.Empty<ApiFootballLiveFixture>();

        var fixtures = new List<ApiFootballLiveFixture>(payload.Response.Count);

        foreach (var item in payload.Response)
        {
            if (item.Fixture is null || item.Teams?.Home is null || item.Teams.Away is null)
                continue;

            fixtures.Add(new ApiFootballLiveFixture(
                item.Fixture.Id,
                item.Fixture.Status?.Short ?? string.Empty,
                item.Goals?.Home,
                item.Goals?.Away,
                item.Teams.Home.Id,
                item.Teams.Away.Id));
        }

        return fixtures;
    }

    public async Task<IReadOnlyList<ApiFootballFixtureEvent>> GetFixtureEventsAsync(
        int fixtureId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"fixtures/events?fixture={fixtureId}",
            cancellationToken);
        LogRateLimit(response);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiFootballResponse<ApiFootballEventItem>>(
            JsonOptions,
            cancellationToken);

        if (payload?.Response is null || payload.Response.Count == 0)
            return Array.Empty<ApiFootballFixtureEvent>();

        var events = new List<ApiFootballFixtureEvent>();

        foreach (var item in payload.Response)
        {
            if (!string.Equals(item.Type, "Goal", StringComparison.OrdinalIgnoreCase))
                continue;

            if (item.Team is null || item.Time is null)
                continue;

            var minute = item.Time.Elapsed + (item.Time.Extra ?? 0);
            var detail = item.Detail ?? string.Empty;

            events.Add(new ApiFootballFixtureEvent(
                minute,
                item.Team.Id,
                item.Player?.Name,
                item.Type ?? "Goal",
                detail));
        }

        return events;
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
        else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogDebug("API-Football: brak nagłówków rate-limit w odpowiedzi.");
        }
    }
}
