namespace Typer.Application.Rankings.DTOs;

public record RankingEntryDto(
    int Position,
    string UserId,
    string DisplayName,
    string? TeamName,
    string? TeamFlagUrl,
    string? PlayerName,
    int TotalPoints,
    int PredictionPoints,
    int TeamBonusPoints,
    int PlayerGoalPoints,
    int TournamentWinnerPoints);
