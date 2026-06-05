using Microsoft.EntityFrameworkCore;
using Typer.Application.Rankings.DTOs;
using Typer.Application.Rankings.Interfaces;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class RankingService : IRankingService
{
    private readonly ApplicationDbContext _context;

    public RankingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RankingEntryDto>> GetLeaderboardAsync(CancellationToken cancellationToken = default)
    {
        var entries = await _context.UserProfiles
            .Include(p => p.SelectedTeam)
            .Include(p => p.SelectedPlayer)
            .OrderByDescending(p => p.TotalPoints)
            .ThenBy(p => p.DisplayName)
            .Select(p => new
            {
                p.UserId,
                p.DisplayName,
                TeamName    = p.SelectedTeam != null ? p.SelectedTeam.Name : null,
                TeamFlagUrl = p.SelectedTeam != null ? p.SelectedTeam.FlagUrl : null,
                PlayerName  = p.SelectedPlayer != null
                    ? p.SelectedPlayer.FirstName + " " + p.SelectedPlayer.LastName
                    : null,
                p.TotalPoints,
                p.PredictionPoints,
                p.TeamBonusPoints,
                p.PlayerGoalPoints,
                p.TournamentWinnerPoints
            })
            .ToListAsync(cancellationToken);

        return entries
            .Select((entry, index) => new RankingEntryDto(
                index + 1,
                entry.UserId,
                entry.DisplayName,
                entry.TeamName,
                entry.TeamFlagUrl,
                entry.PlayerName,
                entry.TotalPoints,
                entry.PredictionPoints,
                entry.TeamBonusPoints,
                entry.PlayerGoalPoints,
                entry.TournamentWinnerPoints))
            .ToList();
    }
}
