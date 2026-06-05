namespace Typer.Application.Predictions.DTOs;

public record MatchDto(
    Guid Id,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    DateTime KickOffUtc,
    string Status,
    int? HomeScore,
    int? AwayScore);

public record PredictionDto(
    Guid Id,
    Guid MatchId,
    int PredictedHomeScore,
    int PredictedAwayScore,
    int? PointsAwarded,
    DateTime SubmittedAt);

public record SubmitPredictionRequest(Guid MatchId, int PredictedHomeScore, int PredictedAwayScore);
