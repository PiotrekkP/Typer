using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Typer.Application.Matches;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Application.Rankings.Interfaces;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class MatchLifecycleService : IMatchLifecycleService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IScoringService _scoringService;
    private readonly IRankingLiveBaselineService _rankingLiveBaselineService;
    private readonly ILogger<MatchLifecycleService> _logger;

    public MatchLifecycleService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IScoringService scoringService,
        IRankingLiveBaselineService rankingLiveBaselineService,
        ILogger<MatchLifecycleService> logger)
    {
        _contextFactory = contextFactory;
        _scoringService = scoringService;
        _rankingLiveBaselineService = rankingLiveBaselineService;
        _logger = logger;
    }

    public async Task AdvanceStatusesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var toStart = await context.Matches
            .Where(m => m.Status == MatchStatus.Scheduled && m.KickOffUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var match in toStart)
        {
            match.Status = MatchStatus.InProgress;
            match.UseManualClock = true;
            match.ClockPhase = MatchClockPhase.FirstHalf;
            match.ClockBaseMinute = 0;
            match.ClockStartedUtc = now;
            match.UpdatedAt = now;
            _logger.LogInformation("Mecz {MatchId} rozpoczęty — status InProgress.", match.Id);
        }

        if (toStart.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            await _rankingLiveBaselineService.SyncWithLiveMatchesAsync(cancellationToken);
        }

        var liveEndsBefore = now - MatchLifecycleRules.LiveDuration;

        var toFinish = await context.Matches
            .Where(m => m.Status == MatchStatus.InProgress
                        && !m.UseManualClock
                        && m.KickOffUtc <= liveEndsBefore)
            .ToListAsync(cancellationToken);

        foreach (var match in toFinish)
        {
            match.Status = MatchStatus.Finished;
            match.UpdatedAt = now;
            _logger.LogInformation("Mecz {MatchId} zakończony po {Hours}h — status Finished.", match.Id, MatchLifecycleRules.LiveDuration.TotalHours);
        }

        if (toFinish.Count > 0)
            await context.SaveChangesAsync(cancellationToken);

        foreach (var match in toFinish)
        {
            if (!match.HomeScore.HasValue || !match.AwayScore.HasValue)
            {
                _logger.LogWarning(
                    "Mecz {MatchId} zakończony automatycznie bez wyniku — pominięto rozliczenie punktów.",
                    match.Id);
                continue;
            }

            var scoringResult = await _scoringService.ScoreMatchAsync(match.Id, cancellationToken);
            if (!scoringResult.Succeeded)
            {
                _logger.LogWarning(
                    "Nie udało się rozliczyć meczu {MatchId}: {Error}",
                    match.Id, scoringResult.Error);
            }
        }

        await _rankingLiveBaselineService.SyncWithLiveMatchesAsync(cancellationToken);
    }
}
