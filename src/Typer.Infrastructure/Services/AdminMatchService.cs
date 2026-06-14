using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Typer.Application.Admin.DTOs;
using Typer.Application.Admin.Interfaces;
using Typer.Application.Common.Models;
using Typer.Application.Matches;
using Typer.Application.Matches.DTOs;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Rankings.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Domain.Entities;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class AdminMatchService : IAdminMatchService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IMatchService _matchService;
    private readonly IScoringService _scoringService;
    private readonly IRankingLiveBaselineService _rankingLiveBaselineService;
    private readonly ILogger<AdminMatchService> _logger;

    public AdminMatchService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IMatchService matchService,
        IScoringService scoringService,
        IRankingLiveBaselineService rankingLiveBaselineService,
        ILogger<AdminMatchService> logger)
    {
        _contextFactory = contextFactory;
        _matchService = matchService;
        _scoringService = scoringService;
        _rankingLiveBaselineService = rankingLiveBaselineService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AdminMatchListItemDto>> GetMatchesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var matches = await context.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.GoalScorers)
            .OrderBy(m => m.KickOffUtc)
            .ToListAsync(cancellationToken);

        return matches.Select(MapListItem).ToList();
    }

    public async Task<AdminMatchDetailDto?> GetMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var match = await context.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.GoalScorers.OrderBy(g => g.Minute))
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

        return match is null ? null : MapDetail(match);
    }

    public Task<Result> UpdateResultAsync(Guid matchId, UpdateMatchResultRequest request, CancellationToken cancellationToken = default)
        => _matchService.UpdateMatchResultAsync(matchId, request, cancellationToken);

    public Task<Result> RescoreAsync(Guid matchId, CancellationToken cancellationToken = default)
        => _scoringService.RescoreMatchAsync(matchId, cancellationToken);

    public async Task<Result> AddGoalScorerAsync(Guid matchId, AddGoalScorerRequest request, CancellationToken cancellationToken = default)
    {
        if (request.PlayerId is null && string.IsNullOrWhiteSpace(request.PlayerName))
            return Result.Failure("Podaj nazwisko strzelca lub wybierz zawodnika z listy.");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var match = await context.Matches
            .Include(m => m.GoalScorers)
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        string playerName = request.PlayerName?.Trim() ?? string.Empty;

        if (request.PlayerId is Guid playerId)
        {
            var player = await context.Players
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);

            if (player is null)
                return Result.Failure("Zawodnik nie istnieje.");

            playerName = $"{player.FirstName} {player.LastName}".Trim();
        }
        else if (string.IsNullOrWhiteSpace(playerName))
        {
            return Result.Failure("Podaj nazwisko strzelca.");
        }

        context.GoalScorers.Add(new GoalScorer
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            PlayerId = request.PlayerId,
            PlayerName = playerName,
            IsHomeTeam = request.IsHomeTeam,
            Minute = request.Minute,
            IsOwnGoal = request.IsOwnGoal,
            IsPenalty = request.IsPenalty,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
        await RescoreIfNeededAsync(match, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveGoalScorerAsync(Guid goalScorerId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var scorer = await context.GoalScorers
            .Include(g => g.Match)
            .FirstOrDefaultAsync(g => g.Id == goalScorerId, cancellationToken);

        if (scorer is null)
            return Result.Failure("Strzelec nie istnieje.");

        var match = scorer.Match;
        context.GoalScorers.Remove(scorer);
        await context.SaveChangesAsync(cancellationToken);
        await RescoreIfNeededAsync(match, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> StartFirstHalfAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var match = await context.Matches.FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);
        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        var wasLive = match.Status == MatchStatus.InProgress;
        var now = DateTime.UtcNow;
        match.UseManualClock = true;
        match.Status = MatchStatus.InProgress;
        match.ClockPhase = MatchClockPhase.FirstHalf;
        match.ClockBaseMinute = 0;
        match.ClockStartedUtc = now;
        if (!wasLive)
        {
            match.LiveApiFixtureId = null;
            match.LiveApiDiscoveryAttempts = 0;
        }
        match.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        if (!wasLive)
            await _rankingLiveBaselineService.SyncWithLiveMatchesAsync(cancellationToken);

        _logger.LogInformation("Admin: mecz {MatchId} — pierwsza połowa.", matchId);
        return Result.Success();
    }

    public async Task<Result> StartHalfTimeAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var match = await context.Matches.FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);
        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        var now = DateTime.UtcNow;
        match.UseManualClock = true;
        match.Status = MatchStatus.InProgress;
        match.ClockPhase = MatchClockPhase.HalfTime;
        match.ClockStartedUtc = null;
        match.ClockBaseMinute = 45;
        match.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin: mecz {MatchId} — przerwa.", matchId);
        return Result.Success();
    }

    public async Task<Result> StartSecondHalfAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var match = await context.Matches.FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);
        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        var now = DateTime.UtcNow;
        match.UseManualClock = true;
        match.Status = MatchStatus.InProgress;
        match.ClockPhase = MatchClockPhase.SecondHalf;
        match.ClockBaseMinute = 45;
        match.ClockStartedUtc = now;
        match.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin: mecz {MatchId} — druga połowa.", matchId);
        return Result.Success();
    }

    public async Task<Result> FinishMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var match = await context.Matches.FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);
        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        if (!match.HomeScore.HasValue || !match.AwayScore.HasValue)
            return Result.Failure("Ustaw wynik przed zakończeniem meczu.");

        var now = DateTime.UtcNow;
        match.UseManualClock = true;
        match.Status = MatchStatus.Finished;
        match.ClockPhase = MatchClockPhase.FullTime;
        match.ClockStartedUtc = null;
        match.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        var scoringResult = await _scoringService.ScoreMatchAsync(matchId, cancellationToken);
        if (!scoringResult.Succeeded)
            return scoringResult;

        await _rankingLiveBaselineService.SyncWithLiveMatchesAsync(cancellationToken);

        _logger.LogInformation("Admin: mecz {MatchId} — zakończony i rozliczony.", matchId);
        return Result.Success();
    }

    public async Task<Result> SetMinuteAsync(Guid matchId, SetMatchMinuteRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Minute is < 0 or > 120)
            return Result.Failure("Minuta musi być między 0 a 120.");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var match = await context.Matches.FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);
        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        var wasLive = match.Status == MatchStatus.InProgress;
        var now = DateTime.UtcNow;
        match.UseManualClock = true;
        match.Status = MatchStatus.InProgress;

        if (request.Minute <= 45)
        {
            match.ClockPhase = MatchClockPhase.FirstHalf;
            match.ClockBaseMinute = request.Minute;
        }
        else
        {
            match.ClockPhase = MatchClockPhase.SecondHalf;
            match.ClockBaseMinute = request.Minute;
        }

        match.ClockStartedUtc = now;
        match.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        if (!wasLive)
            await _rankingLiveBaselineService.SyncWithLiveMatchesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task RescoreIfNeededAsync(Match match, CancellationToken cancellationToken)
    {
        if (match.Status == MatchStatus.Finished)
            await _scoringService.RescoreMatchAsync(match.Id, cancellationToken);
        else if (match.Status == MatchStatus.InProgress
                 && match.HomeScore.HasValue
                 && match.AwayScore.HasValue)
            await _scoringService.RecalculateMatchScoresAsync(match.Id, cancellationToken);
    }

    private static AdminMatchListItemDto MapListItem(Match match)
    {
        var liveMinute = MatchClockRules.GetLiveMinuteDisplay(
            match.UseManualClock,
            match.ClockPhase,
            match.ClockStartedUtc,
            match.ClockBaseMinute,
            match.KickOffUtc);

        return new AdminMatchListItemDto(
            match.Id,
            match.HomeTeam.Name,
            match.AwayTeam.Name,
            match.HomeTeam.FlagUrl,
            match.AwayTeam.FlagUrl,
            MatchLifecycleRules.EnsureUtc(match.KickOffUtc),
            match.Status,
            match.HomeScore,
            match.AwayScore,
            liveMinute,
            match.UseManualClock,
            match.ClockPhase,
            match.GoalScorers.Count);
    }

    private static AdminMatchDetailDto MapDetail(Match match)
    {
        var liveMinute = MatchClockRules.GetLiveMinuteDisplay(
            match.UseManualClock,
            match.ClockPhase,
            match.ClockStartedUtc,
            match.ClockBaseMinute,
            match.KickOffUtc);

        return new AdminMatchDetailDto(
            match.Id,
            match.HomeTeamId,
            match.HomeTeam.Name,
            match.AwayTeamId,
            match.AwayTeam.Name,
            MatchLifecycleRules.EnsureUtc(match.KickOffUtc),
            match.Status,
            match.HomeScore,
            match.AwayScore,
            match.UseManualClock,
            match.ClockPhase,
            match.ClockStartedUtc,
            match.ClockBaseMinute,
            liveMinute,
            match.GoalScorers.Select(g => new AdminGoalScorerDto(
                g.Id,
                g.PlayerId,
                g.PlayerName,
                g.IsHomeTeam,
                g.Minute,
                g.IsOwnGoal,
                g.IsPenalty)).ToList());
    }
}
