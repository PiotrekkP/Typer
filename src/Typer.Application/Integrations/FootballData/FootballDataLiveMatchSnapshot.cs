namespace Typer.Application.Integrations.FootballData;

public sealed record FootballDataLiveMatchSnapshot(
    int MatchId,
    int HomeTeamId,
    int AwayTeamId,
    string HomeTeamTla,
    string AwayTeamTla,
    string HomeTeamName,
    string AwayTeamName,
    int? HomeGoals,
    int? AwayGoals,
    string Status,
    DateTime? UtcDate,
    DateTime? LastUpdatedUtc);
