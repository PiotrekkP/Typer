using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Typer.Application.Matches;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class MatchLifecycleService : IMatchLifecycleService
{
    private readonly ApplicationDbContext _context;
    private readonly IScoringService _scoringService;
    private readonly ILogger<MatchLifecycleService> _logger;

    public MatchLifecycleService(
        ApplicationDbContext context,
        IScoringService scoringService,
        ILogger<MatchLifecycleService> logger)
    {
        _context = context;
        _scoringService = scoringService;
        _logger = logger;
    }

    public async Task AdvanceStatusesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var toStart = await _context.Matches
            .Where(m => m.Status == MatchStatus.Scheduled && m.KickOffUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var match in toStart)
        {
            match.Status = MatchStatus.InProgress;
            match.UpdatedAt = now;
            _logger.LogInformation("Mecz {MatchId} rozpoczęty — status InProgress.", match.Id);
        }

        if (toStart.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

        var liveEndsBefore = now - MatchLifecycleRules.LiveDuration;

        var toFinish = await _context.Matches
            .Where(m => m.Status == MatchStatus.InProgress && m.KickOffUtc <= liveEndsBefore)
            .ToListAsync(cancellationToken);

        foreach (var match in toFinish)
        {
            match.Status = MatchStatus.Finished;
            match.UpdatedAt = now;
            _logger.LogInformation("Mecz {MatchId} zakończony po {Hours}h — status Finished.", match.Id, MatchLifecycleRules.LiveDuration.TotalHours);
        }

        if (toFinish.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

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
    }
}
