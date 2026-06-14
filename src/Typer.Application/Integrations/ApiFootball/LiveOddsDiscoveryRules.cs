namespace Typer.Application.Integrations.ApiFootball;

public static class LiveOddsDiscoveryRules
{
    public static bool CanDiscover(int? homeLiveApiId, int? awayLiveApiId) =>
        homeLiveApiId.HasValue && awayLiveApiId.HasValue;

    public static bool MatchesTeams(
        ApiFootballOddsLiveSnapshot snapshot,
        int homeLiveApiId,
        int awayLiveApiId) =>
        snapshot.HomeTeamApiId == homeLiveApiId
        && snapshot.AwayTeamApiId == awayLiveApiId;

    public static bool IsRecentlyStarted(int? elapsedMinute, int maxElapsedMinutes) =>
        elapsedMinute is int minute && minute >= 0 && minute <= maxElapsedMinutes;

    public static bool IsDiscoverableSnapshot(ApiFootballOddsLiveSnapshot snapshot, int maxElapsedMinutes) =>
        !snapshot.Finished && IsRecentlyStarted(snapshot.ElapsedMinute, maxElapsedMinutes);
}
