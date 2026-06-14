namespace Typer.Application.Integrations.ApiFootball;

public sealed record ApiFootballOddsLiveSnapshot(
    int FixtureId,
    int HomeTeamApiId,
    int AwayTeamApiId,
    int HomeGoals,
    int AwayGoals,
    int? ElapsedMinute,
    string StatusLong,
    string? StatusShort,
    bool OddsFinished,
    bool OddsStopped,
    bool OddsBlocked);
