using Typer.Domain.Entities;

namespace Typer.Application.Integrations.FootballData;

public static class LiveMatchDiscoveryRules
{
    public static bool MatchesTeams(FootballDataLiveMatchSnapshot snapshot, Match match) =>
        MatchesTeamPair(
            snapshot.HomeTeamId,
            snapshot.AwayTeamId,
            snapshot.HomeTeamTla,
            snapshot.AwayTeamTla,
            match.HomeTeam.LiveApiId,
            match.AwayTeam.LiveApiId,
            match.HomeTeam.Code,
            match.AwayTeam.Code);

    public static bool MatchesTeamPair(
        int snapshotHomeId,
        int snapshotAwayId,
        string snapshotHomeTla,
        string snapshotAwayTla,
        int? dbHomeId,
        int? dbAwayId,
        string dbHomeCode,
        string dbAwayCode)
    {
        if (dbHomeId.HasValue && dbAwayId.HasValue)
            return snapshotHomeId == dbHomeId.Value && snapshotAwayId == dbAwayId.Value;

        return TlaMatches(snapshotHomeTla, dbHomeCode)
            && TlaMatches(snapshotAwayTla, dbAwayCode);
    }

    private static bool TlaMatches(string apiTla, string dbCode) =>
        !string.IsNullOrWhiteSpace(apiTla)
        && !string.IsNullOrWhiteSpace(dbCode)
        && string.Equals(apiTla.Trim(), dbCode.Trim(), StringComparison.OrdinalIgnoreCase);
}
