using Microsoft.EntityFrameworkCore;
using Typer.Application.Rankings;
using Typer.Application.Rankings.DTOs;
using Typer.Application.Rankings.Interfaces;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class RankingService : IRankingService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public RankingService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<LeaderboardSnapshotDto> GetLeaderboardSnapshotAsync(
        bool vipOnly = false,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entries = await LoadLeaderboardAsync(vipOnly, cancellationToken, context);
        var liveSessionActive = await context.Matches
            .AnyAsync(m => m.Status == MatchStatus.InProgress, cancellationToken);

        if (!liveSessionActive)
            return new LeaderboardSnapshotDto(entries, new Dictionary<string, int>(), false);

        var baseline = await context.RankingLiveBaselines
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.VipOnly == vipOnly && b.IsActive, cancellationToken);

        if (baseline is null)
            return new LeaderboardSnapshotDto(entries, new Dictionary<string, int>(), false);

        var baselinePoints = RankingLiveBaselineService.DeserializePoints(baseline.PointsJson);
        if (baselinePoints is null || baselinePoints.Count == 0)
            return new LeaderboardSnapshotDto(entries, new Dictionary<string, int>(), true);

        var deltas = RankingDeltaRules.ComputeDeltas(baselinePoints, entries);
        return new LeaderboardSnapshotDto(entries, deltas, true);
    }

    public Task<IReadOnlyList<RankingEntryDto>> GetLeaderboardAsync(
        bool vipOnly = false,
        CancellationToken cancellationToken = default)
    {
        return LoadLeaderboardAsync(vipOnly, cancellationToken);
    }

    private async Task<IReadOnlyList<RankingEntryDto>> LoadLeaderboardAsync(
        bool vipOnly,
        CancellationToken cancellationToken,
        ApplicationDbContext? context = null)
    {
        var ownsContext = context is null;
        context ??= await _contextFactory.CreateDbContextAsync(cancellationToken);

        try
        {
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
        finally
        {
            if (ownsContext)
                await context.DisposeAsync();
        }
    }
}
