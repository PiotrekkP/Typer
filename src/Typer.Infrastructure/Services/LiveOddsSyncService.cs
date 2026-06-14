using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typer.Application.Integrations.ApiFootball;
using Typer.Application.Rankings.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Domain.Entities;
using Typer.Domain.Enums;
using Typer.Infrastructure.Integrations.ApiFootball;
using Typer.Infrastructure.Options;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public sealed class LiveOddsSyncService : ILiveOddsSyncService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IApiFootballClient _apiFootballClient;
    private readonly IScoringService _scoringService;
    private readonly IRankingLiveBaselineService _rankingLiveBaselineService;
    private readonly ApiFootballOptions _options;
    private readonly ILogger<LiveOddsSyncService> _logger;

    public LiveOddsSyncService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IApiFootballClient apiFootballClient,
        IScoringService scoringService,
        IRankingLiveBaselineService rankingLiveBaselineService,
        IOptions<ApiFootballOptions> options,
        ILogger<LiveOddsSyncService> logger)
    {
        _contextFactory = contextFactory;
        _apiFootballClient = apiFootballClient;
        _scoringService = scoringService;
        _rankingLiveBaselineService = rankingLiveBaselineService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<LiveOddsSyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var liveMatches = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.Status == MatchStatus.InProgress)
            .ToListAsync(cancellationToken);

        if (liveMatches.Count == 0)
        {
            return new LiveOddsSyncResult(
                ApiCalled: false,
                MatchesUpdated: 0,
                MatchesFinished: 0,
                HadInProgressMatch: false,
                NeedsDiscoveryPoll: false,
                SkipReason: "no_in_progress_matches");
        }

        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new LiveOddsSyncResult(
                ApiCalled: false,
                MatchesUpdated: 0,
                MatchesFinished: 0,
                HadInProgressMatch: true,
                NeedsDiscoveryPoll: false,
                SkipReason: "disabled");
        }

        var apiCalled = false;
        var matchesUpdated = 0;
        var matchesFinished = 0;
        var updatedMatchIds = new List<Guid>();
        var finishedMatchIds = new List<Guid>();

        var needsDiscovery = liveMatches.Where(NeedsDiscovery).ToList();
        if (needsDiscovery.Count > 0)
        {
            try
            {
                var feed = await _apiFootballClient.GetOddsLiveFeedAsync(_options.LeagueId, cancellationToken);
                apiCalled = true;

                foreach (var match in needsDiscovery)
                {
                    if (match.LiveApiDiscoveryAttempts >= _options.DiscoveryMaxAttempts)
                        continue;

                    var homeApiId = match.HomeTeam.LiveApiId!.Value;
                    var awayApiId = match.AwayTeam.LiveApiId!.Value;

                    var found = feed.FirstOrDefault(snapshot =>
                        LiveOddsDiscoveryRules.IsDiscoverableSnapshot(snapshot, _options.DiscoveryMaxElapsedMinutes)
                        && LiveOddsDiscoveryRules.MatchesTeams(snapshot, homeApiId, awayApiId));

                    match.LiveApiDiscoveryAttempts++;

                    if (found is null)
                    {
                        _logger.LogDebug(
                            "Discovery odds/live: brak meczu {Home}–{Away} (próba {Attempt}/{Max}).",
                            match.HomeTeam.Name,
                            match.AwayTeam.Name,
                            match.LiveApiDiscoveryAttempts,
                            _options.DiscoveryMaxAttempts);
                        continue;
                    }

                    match.LiveApiFixtureId = found.FixtureId;
                    if (ApplySnapshot(match, found))
                        updatedMatchIds.Add(match.Id);

                    _logger.LogInformation(
                        "Discovery odds/live: {Home}–{Away} → fixture {FixtureId}.",
                        match.HomeTeam.Name,
                        match.AwayTeam.Name,
                        found.FixtureId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nie udało się pobrać feedu odds/live (discovery).");
                return new LiveOddsSyncResult(
                    ApiCalled: apiCalled,
                    MatchesUpdated: 0,
                    MatchesFinished: 0,
                    HadInProgressMatch: true,
                    NeedsDiscoveryPoll: StillNeedsDiscovery(liveMatches),
                    SkipReason: "api_error");
            }
        }

        var trackedMatches = liveMatches
            .Where(m => m.LiveApiFixtureId.HasValue)
            .ToList();

        foreach (var match in trackedMatches)
        {
            if (updatedMatchIds.Contains(match.Id))
                continue;

            try
            {
                var snapshot = await _apiFootballClient.GetOddsLiveByFixtureAsync(
                    match.LiveApiFixtureId!.Value,
                    cancellationToken);
                apiCalled = true;

                if (snapshot is null)
                {
                    _logger.LogWarning(
                        "Brak danych odds/live dla fixture {FixtureId} (mecz {MatchId}).",
                        match.LiveApiFixtureId,
                        match.Id);
                    continue;
                }

                if (ApplySnapshot(match, snapshot))
                    updatedMatchIds.Add(match.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Nie udało się pobrać odds/live dla fixture {FixtureId}.",
                    match.LiveApiFixtureId);
            }
        }

        if (updatedMatchIds.Count > 0)
            await context.SaveChangesAsync(cancellationToken);

        foreach (var matchId in updatedMatchIds.Distinct())
        {
            var match = liveMatches.First(m => m.Id == matchId);
            if (match.Status == MatchStatus.Finished)
                finishedMatchIds.Add(matchId);

            var scoringResult = await _scoringService.RecalculateMatchScoresAsync(matchId, cancellationToken);
            if (!scoringResult.Succeeded)
            {
                _logger.LogWarning(
                    "Nie udało się przeliczyć punktów live dla meczu {MatchId}: {Error}",
                    matchId,
                    scoringResult.Error);
            }
        }

        foreach (var matchId in finishedMatchIds.Distinct())
        {
            matchesFinished++;
            var finalScore = await _scoringService.ScoreMatchAsync(matchId, cancellationToken);
            if (!finalScore.Succeeded)
            {
                _logger.LogWarning(
                    "Nie udało się rozliczyć meczu {MatchId}: {Error}",
                    matchId,
                    finalScore.Error);
            }
        }

        if (finishedMatchIds.Count > 0)
            await _rankingLiveBaselineService.SyncWithLiveMatchesAsync(cancellationToken);

        matchesUpdated = updatedMatchIds.Distinct().Count();

        return new LiveOddsSyncResult(
            ApiCalled: apiCalled,
            MatchesUpdated: matchesUpdated,
            MatchesFinished: matchesFinished,
            HadInProgressMatch: true,
            NeedsDiscoveryPoll: StillNeedsDiscovery(liveMatches),
            SkipReason: null);
    }

    private bool NeedsDiscovery(Match match) =>
        !match.LiveApiFixtureId.HasValue
        && LiveOddsDiscoveryRules.CanDiscover(match.HomeTeam.LiveApiId, match.AwayTeam.LiveApiId)
        && match.LiveApiDiscoveryAttempts < _options.DiscoveryMaxAttempts;

    private bool StillNeedsDiscovery(IEnumerable<Match> liveMatches) =>
        liveMatches.Any(NeedsDiscovery);

    private bool ApplySnapshot(Match match, ApiFootballOddsLiveSnapshot snapshot)
    {
        var now = DateTime.UtcNow;
        var previousHome = match.HomeScore;
        var previousAway = match.AwayScore;
        var previousPhase = match.ClockPhase;
        var previousMinute = match.ClockBaseMinute;
        var phase = ApiFootballClockMapper.MapPhase(snapshot.StatusLong, snapshot.Finished);
        var finished = snapshot.Finished || phase == MatchClockPhase.FullTime;

        match.HomeScore = snapshot.HomeGoals;
        match.AwayScore = snapshot.AwayGoals;
        match.UseManualClock = true;
        match.ClockPhase = phase;
        match.UpdatedAt = now;

        if (snapshot.ElapsedMinute is int minute)
        {
            match.ClockBaseMinute = minute;
            match.ClockStartedUtc = phase is MatchClockPhase.FirstHalf or MatchClockPhase.SecondHalf
                ? now
                : null;
        }

        if (finished)
        {
            match.Status = MatchStatus.Finished;
            match.ClockPhase = MatchClockPhase.FullTime;
            match.ClockStartedUtc = null;
            _logger.LogInformation(
                "Mecz {MatchId} zakończony wg odds/live (fixture {FixtureId}, wynik {Home}-{Away}).",
                match.Id,
                snapshot.FixtureId,
                snapshot.HomeGoals,
                snapshot.AwayGoals);
        }

        return previousHome != match.HomeScore
            || previousAway != match.AwayScore
            || previousPhase != match.ClockPhase
            || previousMinute != match.ClockBaseMinute
            || finished;
    }
}
