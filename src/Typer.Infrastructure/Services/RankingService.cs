using Microsoft.EntityFrameworkCore;
using Typer.Application.Rankings.DTOs;
using Typer.Application.Rankings.Interfaces;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class RankingService : IRankingService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public RankingService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<RankingEntryDto>> GetLeaderboardAsync(
        bool vipOnly = false,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.UserProfiles.AsQueryable();
        if (vipOnly)
            query = query.Where(p => p.VipUser);

        var entries = await query
            .Include(p => p.SelectedTeam)
            .Include(p => p.SelectedPlayer)
            .OrderByDescending(p => p.TotalPoints)
            .ThenBy(p => p.DisplayName)
            .ThenBy(p => p.UserId)
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
