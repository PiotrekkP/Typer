using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Typer.Application.Common.Models;
using Typer.Application.Matches;
using Typer.Application.Matches.DTOs;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Rankings.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class MatchService : IMatchService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IScoringService _scoringService;
    private readonly IRankingLiveBaselineService _rankingLiveBaselineService;
    private readonly ILogger<MatchService> _logger;

    public MatchService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IScoringService scoringService,
        IRankingLiveBaselineService rankingLiveBaselineService,
        ILogger<MatchService> logger)
    {
        _contextFactory = contextFactory;
        _scoringService = scoringService;
        _rankingLiveBaselineService = rankingLiveBaselineService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RoundWithMatchesDto>> GetRoundsWithMatchesAsync(
        string? userId = null,
        MatchRoundsScope scope = MatchRoundsScope.Active,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var rounds = await context.Rounds
            .Include(r => r.Matches)
                .ThenInclude(m => m.HomeTeam)
            .Include(r => r.Matches)
                .ThenInclude(m => m.AwayTeam)
            .Include(r => r.Matches)
                .ThenInclude(m => m.GoalScorers.OrderBy(g => g.Minute))
            .OrderBy(r => r.OrderNumber)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        Dictionary<Guid, (int? Home, int? Away, int? Points, int? Base, int? TeamBonus, int? PlayerGoal)> predictions = [];

        var scopedMatches = rounds
            .SelectMany(r => r.Matches)
            .Where(m => MatchesScope(m, scope))
            .Select(m => m.Id)
            .ToList();

        if (userId is not null && scopedMatches.Count > 0)
        {
            predictions = await context.Predictions
                .Where(p => p.UserId == userId && scopedMatches.Contains(p.MatchId))
                .AsNoTracking()
                .ToDictionaryAsync(
                    p => p.MatchId,
                    p => ((int?)p.PredictedHomeScore, (int?)p.PredictedAwayScore,
                          p.PointsAwarded, p.BasePoints, p.TeamBonusPoints, p.PlayerGoalPoints),
                    cancellationToken);
        }

        var roundDtos = rounds
            .Select(round =>
            {
                var scoped = round.Matches.Where(m => MatchesScope(m, scope)).ToList();
                var orderedMatches = scope == MatchRoundsScope.Results
                    ? MatchResultsOrderingRules.OrderForResultsPage(
                        scoped,
                        m => m.Status,
                        m => m.KickOffUtc)
                    : scoped.OrderBy(m => m.KickOffUtc);

                var matches = orderedMatches
                    .Select(match =>
                    {
                        predictions.TryGetValue(match.Id, out var pred);
                        return MapMatchDetailDto(match, pred);
                    })
                    .ToList();

                return new RoundWithMatchesDto(round.Id, round.Name, round.OrderNumber, matches);
            })
            .Where(round => round.Matches.Count > 0);

        if (scope == MatchRoundsScope.Results)
        {
            return roundDtos
                .OrderByDescending(round =>
                    round.Matches.Any(m => m.Status == nameof(MatchStatus.InProgress)))
                .ThenByDescending(round =>
                    round.Matches.Max(m => MatchLifecycleRules.EnsureUtc(m.KickOffUtc)))
                .ToList();
        }

        return roundDtos.ToList();
    }

    private static bool MatchesScope(Typer.Domain.Entities.Match match, MatchRoundsScope scope)
    {
        var effective = MatchLifecycleRules.GetEffectiveStatus(match.Status, match.KickOffUtc);
        return scope switch
        {
            MatchRoundsScope.Active => effective == MatchStatus.Scheduled,
            MatchRoundsScope.Results => effective is MatchStatus.InProgress or MatchStatus.Finished,
            _ => false
        };
    }

    public async Task<IReadOnlyList<MatchDetailDto>> GetUpcomingMatchesAsync(
        int count = 5,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var matches = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.GoalScorers.OrderBy(g => g.Minute))
            .Where(m => m.Status == MatchStatus.Scheduled || m.Status == MatchStatus.InProgress)
            .OrderBy(m => m.Status == MatchStatus.InProgress ? 0 : 1)
            .ThenBy(m => m.KickOffUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var openMatches = matches
            .Where(m => MatchesScope(m, MatchRoundsScope.Active))
            .Take(count)
            .ToList();

        return await MapToDetailDtos(context, openMatches, userId, cancellationToken);
    }

    public async Task<IReadOnlyList<MatchDetailDto>> GetRecentFinishedMatchesAsync(
        int count = 5,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var matches = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.GoalScorers.OrderBy(g => g.Minute))
            .Where(m => m.Status == MatchStatus.Finished)
            .OrderByDescending(m => m.KickOffUtc)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return await MapToDetailDtos(context, matches, userId, cancellationToken);
    }

    public async Task<IReadOnlyList<MatchDetailDto>> GetTeamRecentMatchesAsync(
        Guid teamId,
        int count = 10,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var candidates = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.GoalScorers.OrderBy(g => g.Minute))
            .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                        && m.Status != MatchStatus.Cancelled)
            .OrderByDescending(m => m.KickOffUtc)
            .Take(Math.Max(count, 10) * 3)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var matches = candidates
            .Where(m => MatchesScope(m, MatchRoundsScope.Results))
            .Take(count)
            .ToList();

        return await MapToDetailDtos(context, matches, userId, cancellationToken);
    }

    private static async Task<IReadOnlyList<MatchDetailDto>> MapToDetailDtos(
        ApplicationDbContext context,
        List<Typer.Domain.Entities.Match> matches,
        string? userId,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, (int? Home, int? Away, int? Points, int? Base, int? TeamBonus, int? PlayerGoal)> predictions = [];

        if (userId is not null && matches.Count > 0)
        {
            var ids = matches.Select(m => m.Id).ToList();
            predictions = await context.Predictions
                .Where(p => p.UserId == userId && ids.Contains(p.MatchId))
                .AsNoTracking()
                .ToDictionaryAsync(
                    p => p.MatchId,
                    p => ((int?)p.PredictedHomeScore, (int?)p.PredictedAwayScore,
                          p.PointsAwarded, p.BasePoints, p.TeamBonusPoints, p.PlayerGoalPoints),
                    cancellationToken);
        }

        return matches.Select(match =>
        {
            predictions.TryGetValue(match.Id, out var pred);
            return MapMatchDetailDto(match, pred);
        }).ToList();
    }

    private static MatchDetailDto MapMatchDetailDto(
        Typer.Domain.Entities.Match match,
        (int? Home, int? Away, int? Points, int? Base, int? TeamBonus, int? PlayerGoal) pred)
    {
        var effectiveStatus = MatchLifecycleRules.GetEffectiveStatusName(match.Status, match.KickOffUtc);
        var predictionStatus = MatchLifecycleRules.GetPredictionStatus(match.Status, match.KickOffUtc);
        var clockKickOff = match.LiveApiKickOffUtc ?? match.KickOffUtc;
        var liveMinute = match.UseManualClock
            ? MatchClockRules.GetLiveMinuteDisplay(
                match.UseManualClock,
                match.ClockPhase,
                match.ClockStartedUtc,
                match.ClockBaseMinute,
                match.KickOffUtc)
            : MatchClockRules.GetLiveMinuteDisplay(
                useManualClock: false,
                match.ClockPhase,
                match.ClockStartedUtc,
                match.ClockBaseMinute,
                clockKickOff);

        return new MatchDetailDto(
            match.Id,
            match.HomeTeamId,
            match.HomeTeam.Name,
            match.HomeTeam.Code,
            match.HomeTeam.FlagUrl,
            match.AwayTeamId,
            match.AwayTeam.Name,
            match.AwayTeam.Code,
            match.AwayTeam.FlagUrl,
            MatchLifecycleRules.EnsureUtc(match.KickOffUtc),
            effectiveStatus,
            match.HomeScore,
            match.AwayScore,
            match.GoalScorers
                .Select(g => new GoalScorerDto(g.Id, g.PlayerName, g.IsHomeTeam, g.Minute, g.IsOwnGoal, g.IsPenalty))
                .ToList(),
            pred.Home,
            pred.Away,
            pred.Points,
            pred.Base,
            pred.TeamBonus,
            pred.PlayerGoal,
            predictionStatus,
            match.UseManualClock,
            match.ClockPhase.ToString(),
            match.ClockStartedUtc,
            match.ClockBaseMinute,
            liveMinute,
            match.LiveApiKickOffUtc);
    }

    public async Task<Result> UpdateMatchResultAsync(
        Guid matchId,
        UpdateMatchResultRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var match = await context.Matches
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        var previousStatus = match.Status;

        var enteredLive = previousStatus != MatchStatus.InProgress && request.Status == MatchStatus.InProgress;
        var finishedLive = previousStatus == MatchStatus.InProgress && request.Status == MatchStatus.Finished;

        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        match.Status    = request.Status;
        match.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        if (enteredLive)
            await _rankingLiveBaselineService.SyncWithLiveMatchesAsync(cancellationToken);

        if (request.Status == MatchStatus.InProgress
            || match.Status == MatchStatus.InProgress)
        {
            await _scoringService.RecalculateMatchScoresAsync(matchId, cancellationToken);
        }

        if (request.Status == MatchStatus.Finished)
        {
            var scoringResult = await _scoringService.ScoreMatchAsync(matchId, cancellationToken);

            if (!scoringResult.Succeeded)
            {
                _logger.LogWarning(
                    "Nie udało się przyznać punktów za mecz {MatchId}: {Error}",
                    matchId, scoringResult.Error);
            }
            else
            {
                _logger.LogInformation(
                    "Punkty za mecz {MatchId} przyznane automatycznie.", matchId);
            }
        }

        if (finishedLive)
            await _rankingLiveBaselineService.SyncWithLiveMatchesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<bool> HasInProgressMatchesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Matches.AnyAsync(m => m.Status == MatchStatus.InProgress, cancellationToken);
    }

    public async Task<IReadOnlyList<ResultsMatchOptionDto>> GetResultsMatchOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var candidates = await context.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Round)
            .Where(m => m.Status != MatchStatus.Cancelled)
            .ToListAsync(cancellationToken);

        return MatchResultsOrderingRules
            .OrderForResultsPage(
                candidates.Where(m => MatchesScope(m, MatchRoundsScope.Results)),
                m => m.Status,
                m => m.KickOffUtc)
            .Select(m => new ResultsMatchOptionDto(
                m.Id,
                m.Round != null ? m.Round.Name : "Inne",
                m.Round != null ? m.Round.OrderNumber : int.MaxValue,
                m.HomeTeamId,
                m.HomeTeam.Name,
                m.HomeTeam.FlagUrl,
                m.AwayTeamId,
                m.AwayTeam.Name,
                m.AwayTeam.FlagUrl,
                m.KickOffUtc,
                MatchLifecycleRules.GetEffectiveStatusName(m.Status, m.KickOffUtc),
                m.HomeScore,
                m.AwayScore))
            .ToList();
    }

    public async Task<IReadOnlyList<MatchPredictionResultDto>> GetMatchPredictionResultsAsync(
        Guid matchId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var match = await context.Matches
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

        if (match is null || !MatchesScope(match, MatchRoundsScope.Results))
            return [];

        var predictions = await context.Predictions
            .AsNoTracking()
            .Where(p => p.MatchId == matchId)
            .ToListAsync(cancellationToken);

        if (predictions.Count == 0)
            return [];

        var userIds = predictions.Select(p => p.UserId).Distinct().ToList();
        var displayNames = await context.UserProfiles
            .AsNoTracking()
            .Where(p => userIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, p => p.DisplayName, cancellationToken);

        return predictions
            .Select(p => new MatchPredictionResultDto(
                p.UserId,
                displayNames.GetValueOrDefault(p.UserId) ?? p.UserId,
                p.PredictedHomeScore,
                p.PredictedAwayScore,
                p.PointsAwarded,
                p.BasePoints,
                p.TeamBonusPoints,
                p.PlayerGoalPoints))
            .OrderByDescending(r => r.PointsAwarded ?? -1)
            .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
