using Typer.Domain.Enums;

namespace Typer.Application.Matches.DTOs;

public record GoalScorerDto(
    string PlayerName,
    bool IsHomeTeam,
    int Minute,
    bool IsOwnGoal,
    bool IsPenalty);

/// <summary>
/// PredictionStatus: "Open" — przed kickoffem (można typować/edytować);
/// "Locked" — mecz się zaczął lub status niezaktualizowany, ale czas minął;
/// "Scored" — mecz zakończony, punkty przyznane.
/// </summary>
public record MatchDetailDto(
    Guid Id,
    Guid HomeTeamId,
    string HomeTeamName,
    string HomeTeamCode,
    string? HomeTeamFlagUrl,
    Guid AwayTeamId,
    string AwayTeamName,
    string AwayTeamCode,
    string? AwayTeamFlagUrl,
    DateTime KickOffUtc,
    string Status,
    int? HomeScore,
    int? AwayScore,
    IReadOnlyList<GoalScorerDto> GoalScorers,
    int? PredictedHomeScore,
    int? PredictedAwayScore,
    int? PointsAwarded,
    int? BasePoints,
    int? TeamBonusPoints,
    int? PlayerGoalPoints,
    string PredictionStatus);

/// <summary>
/// Żądanie aktualizacji wyniku meczu. Status = 2 (Finished) automatycznie wyzwala
/// przyznawanie punktów za predykcje.
/// </summary>
public record UpdateMatchResultRequest(
    int HomeScore,
    int AwayScore,
    MatchStatus Status);

public record RoundWithMatchesDto(
    Guid Id,
    string Name,
    int OrderNumber,
    IReadOnlyList<MatchDetailDto> Matches);
