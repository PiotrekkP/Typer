namespace Typer.Application.Integrations.ApiFootball;

public interface IApiFootballClient
{
    Task<IReadOnlyList<ApiFootballOddsLiveSnapshot>> GetOddsLiveFeedAsync(
        int? leagueId = null,
        CancellationToken cancellationToken = default);

    Task<ApiFootballOddsLiveSnapshot?> GetOddsLiveByFixtureAsync(
        int fixtureId,
        CancellationToken cancellationToken = default);
}
