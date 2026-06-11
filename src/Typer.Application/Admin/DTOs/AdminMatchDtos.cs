using Typer.Application.Matches.DTOs;
using Typer.Domain.Enums;

namespace Typer.Application.Admin.DTOs;

public record AdminMatchListItemDto(
    Guid Id,
    string HomeTeamName,
    string AwayTeamName,
    string? HomeTeamFlagUrl,
    string? AwayTeamFlagUrl,
    DateTime KickOffUtc,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore,
    string LiveMinuteDisplay,
    bool UseManualClock,
    MatchClockPhase ClockPhase,
    int GoalScorerCount);

public record AdminMatchDetailDto(
    Guid Id,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    DateTime KickOffUtc,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore,
    bool UseManualClock,
    MatchClockPhase ClockPhase,
    DateTime? ClockStartedUtc,
    int ClockBaseMinute,
    string LiveMinuteDisplay,
    IReadOnlyList<AdminGoalScorerDto> GoalScorers);

public record AdminGoalScorerDto(
    Guid Id,
    Guid? PlayerId,
    string PlayerName,
    bool IsHomeTeam,
    int Minute,
    bool IsOwnGoal,
    bool IsPenalty);

public record AddGoalScorerRequest(
    Guid? PlayerId,
    string PlayerName,
    bool IsHomeTeam,
    int Minute,
    bool IsOwnGoal,
    bool IsPenalty);

public record SetMatchMinuteRequest(int Minute);
