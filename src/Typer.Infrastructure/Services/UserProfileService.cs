using Microsoft.EntityFrameworkCore;
using Typer.Application.UserProfile.DTOs;
using Typer.Application.UserProfile.Interfaces;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly ApplicationDbContext _context;

    public UserProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserSelectionDto?> GetSelectionAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _context.UserProfiles
            .Include(p => p.SelectedTeam)
            .Include(p => p.SelectedPlayer)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
            return null;

        return new UserSelectionDto(
            profile.SelectedTeamId,
            profile.SelectedTeam?.Name,
            profile.SelectedTeam?.Code,
            profile.SelectedTeam?.FlagUrl,
            profile.SelectedPlayerId,
            profile.SelectedPlayer is null ? null
                : $"{profile.SelectedPlayer.FirstName} {profile.SelectedPlayer.LastName}",
            profile.SelectedPlayer?.JerseyNumber,
            profile.TotalPoints);
    }

    public async Task<UserPublicProfileDto?> GetPublicProfileAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _context.UserProfiles
            .Include(p => p.SelectedTeam)
            .Include(p => p.SelectedPlayer)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
            return null;

        // Pozycja w rankingu (ile osób ma więcej punktów + 1)
        var position = await _context.UserProfiles
            .AsNoTracking()
            .CountAsync(p => p.TotalPoints > profile.TotalPoints, cancellationToken) + 1;

        // Historia typów (zakończone mecze z przyznanymi punktami)
        var history = await _context.Predictions
            .Where(p => p.UserId == userId && p.PointsAwarded.HasValue)
            .Include(p => p.Match)
                .ThenInclude(m => m.HomeTeam)
            .Include(p => p.Match)
                .ThenInclude(m => m.AwayTeam)
            .AsNoTracking()
            .OrderByDescending(p => p.Match.KickOffUtc)
            .Select(p => new UserPredictionHistoryDto(
                p.MatchId,
                p.Match.HomeTeam.Name,
                p.Match.HomeTeam.FlagUrl,
                p.Match.AwayTeam.Name,
                p.Match.AwayTeam.FlagUrl,
                p.Match.KickOffUtc,
                p.Match.HomeScore,
                p.Match.AwayScore,
                p.PredictedHomeScore,
                p.PredictedAwayScore,
                p.PointsAwarded,
                p.BasePoints,
                p.TeamBonusPoints,
                p.PlayerGoalPoints))
            .ToListAsync(cancellationToken);

        var playerName = profile.SelectedPlayer is null ? null
            : $"{profile.SelectedPlayer.FirstName} {profile.SelectedPlayer.LastName}";

        return new UserPublicProfileDto(
            profile.UserId,
            profile.DisplayName,
            position,
            profile.TotalPoints,
            profile.PredictionPoints,
            profile.TeamBonusPoints,
            profile.PlayerGoalPoints,
            profile.TournamentWinnerPoints,
            profile.SelectedTeamId,
            profile.SelectedTeam?.Name,
            profile.SelectedTeam?.FlagUrl,
            profile.SelectedPlayerId,
            playerName,
            profile.SelectedPlayer?.JerseyNumber,
            history);
    }
}
