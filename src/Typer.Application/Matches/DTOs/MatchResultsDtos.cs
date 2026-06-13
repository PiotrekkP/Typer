namespace Typer.Application.Matches.DTOs;

public record ResultsMatchOptionDto(
    Guid MatchId,
    string RoundName,
    int RoundOrderNumber,
    string HomeTeamName,
    string? HomeTeamFlagUrl,
    string AwayTeamName,
    string? AwayTeamFlagUrl,
    DateTime KickOffUtc,
    string Status,
    int? HomeScore,
    int? AwayScore);

public record MatchPredictionResultDto(
    string UserId,
    string DisplayName,
    int PredictedHomeScore,
    int PredictedAwayScore,
    int? PointsAwarded,
    int? BasePoints,
    int? TeamBonusPoints,
    int? PlayerGoalPoints);
