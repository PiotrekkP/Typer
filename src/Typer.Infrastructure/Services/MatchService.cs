using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Typer.Application.Common.Models;
using Typer.Application.Matches;
using Typer.Application.Matches.DTOs;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class MatchService : IMatchService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IScoringService _scoringService;
    private readonly ILogger<MatchService> _logger;

    public MatchService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IScoringService scoringService,
        ILogger<MatchService> logger)
    {
        _contextFactory = contextFactory;
        _scoringService = scoringService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RoundWithMatchesDto>> GetRoundsWithMatchesAsync(
        string? userId = null,
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

        if (userId is not null)
        {
            var matchIds = rounds.SelectMany(r => r.Matches).Select(m => m.Id).ToList();

            predictions = await context.Predictions
                .Where(p => p.UserId == userId && matchIds.Contains(p.MatchId))
                .AsNoTracking()
                .ToDictionaryAsync(
                    p => p.MatchId,
                    p => ((int?)p.PredictedHomeScore, (int?)p.PredictedAwayScore,
                          p.PointsAwarded, p.BasePoints, p.TeamBonusPoints, p.PlayerGoalPoints),
                    cancellationToken);
        }

        return rounds.Select(round => new RoundWithMatchesDto(
            round.Id,
            round.Name,
            round.OrderNumber,
            round.Matches
                .OrderBy(m => m.KickOffUtc)
                .Select(match =>
                {
                    predictions.TryGetValue(match.Id, out var pred);
                    return MapMatchDetailDto(match, pred);
                })
                .ToList()
        )).ToList();
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
            .OrderBy(m => m.KickOffUtc)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return await MapToDetailDtos(context, matches, userId, cancellationToken);
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
        var liveMinute = MatchClockRules.GetLiveMinuteDisplay(
            match.UseManualClock,
            match.ClockPhase,
            match.ClockStartedUtc,
            match.ClockBaseMinute,
            match.KickOffUtc);

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
            liveMinute);
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

        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        match.Status    = request.Status;
        match.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

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

        return Result.Success();
    }
}
