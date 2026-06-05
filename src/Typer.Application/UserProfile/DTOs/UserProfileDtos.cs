namespace Typer.Application.UserProfile.DTOs;

public record UserSelectionDto(
    Guid? SelectedTeamId,
    string? SelectedTeamName,
    string? SelectedTeamCode,
    string? SelectedTeamFlagUrl,
    Guid? SelectedPlayerId,
    string? SelectedPlayerName,
    int? SelectedPlayerNumber,
    string? SelectedPlayerPhotoUrl,
    int TotalPoints,
    int TeamBonusPoints,
    int PlayerGoalPoints);

public record UserPredictionHistoryDto(
    Guid MatchId,
    string HomeTeamName,
    string? HomeTeamFlagUrl,
    string AwayTeamName,
    string? AwayTeamFlagUrl,
    DateTime KickOffUtc,
    int? HomeScore,
    int? AwayScore,
    int PredictedHomeScore,
    int PredictedAwayScore,
    int? PointsAwarded,
    int? BasePoints,
    int? TeamBonusPoints,
    int? PlayerGoalPoints);

public record UserPublicProfileDto(
    string UserId,
    string DisplayName,
    int Position,
    int TotalPoints,
    int PredictionPoints,
    int TeamBonusPoints,
    int PlayerGoalPoints,
    int TournamentWinnerPoints,
    Guid? SelectedTeamId,
    string? SelectedTeamName,
    string? SelectedTeamFlagUrl,
    Guid? SelectedPlayerId,
    string? SelectedPlayerName,
    int? SelectedPlayerNumber,
    IReadOnlyList<UserPredictionHistoryDto> PredictionHistory);
