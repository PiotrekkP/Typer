namespace Typer.Application.Integrations.ApiFootball;

public sealed record ApiFootballLiveFixture(
    int FixtureId,
    string StatusShort,
    int? HomeGoals,
    int? AwayGoals,
    int HomeTeamId,
    int AwayTeamId);

public sealed record ApiFootballFixtureEvent(
    int Minute,
    int TeamId,
    string? PlayerName,
    string Type,
    string Detail);
