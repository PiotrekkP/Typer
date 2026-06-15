namespace Typer.Application.Integrations.FootballData;

public interface IFootballDataClient
{
    Task<IReadOnlyList<FootballDataLiveMatchSnapshot>> GetLiveMatchesAsync(
        CancellationToken cancellationToken = default);

    Task<FootballDataLiveMatchSnapshot?> GetMatchAsync(
        int matchId,
        CancellationToken cancellationToken = default);
}
