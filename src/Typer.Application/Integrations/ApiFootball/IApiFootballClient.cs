namespace Typer.Application.Integrations.ApiFootball;

public interface IApiFootballClient
{
    Task<IReadOnlyList<ApiFootballLiveFixture>> GetLiveFixturesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApiFootballFixtureEvent>> GetFixtureEventsAsync(
        int fixtureId,
        CancellationToken cancellationToken = default);
}
