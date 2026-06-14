namespace Typer.Application.Integrations.ApiFootball;

public sealed record ApiFootballOddsLiveSnapshot(
    int FixtureId,
    int HomeTeamApiId,
    int AwayTeamApiId,
    int HomeGoals,
    int AwayGoals,
    int? ElapsedMinute,
    string StatusLong,
    bool Finished,
    bool Stopped,
    bool Blocked);
