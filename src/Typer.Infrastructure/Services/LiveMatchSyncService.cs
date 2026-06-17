using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typer.Application.Integrations.FootballData;
using Typer.Application.Matches;
using Typer.Application.Rankings.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Domain.Entities;
using Typer.Domain.Enums;
using Typer.Infrastructure.Options;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

/// <summary>
/// Synchronizuje wyniki na żywo z football-data.org (feed matches?status=LIVE).
/// </summary>
public sealed class LiveMatchSyncService : ILiveMatchSyncService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IFootballDataClient _footballDataClient;
    private readonly IScoringService _scoringService;
    private readonly IRankingLiveBaselineService _rankingLiveBaselineService;
    private readonly FootballDataOptions _options;
    private readonly ILogger<LiveMatchSyncService> _logger;

    public LiveMatchSyncService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IFootballDataClient footballDataClient,
        IScoringService scoringService,
        IRankingLiveBaselineService rankingLiveBaselineService,
        IOptions<FootballDataOptions> options,
        ILogger<LiveMatchSyncService> logger)
    {
        _contextFactory = contextFactory;
        _footballDataClient = footballDataClient;
        _scoringService = scoringService;
        _rankingLiveBaselineService = rankingLiveBaselineService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<LiveMatchSyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var liveMatches = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.Status == MatchStatus.InProgress)
            .ToListAsync(cancellationToken);

        if (liveMatches.Count == 0)
        {
            return new LiveMatchSyncResult(
                ApiCalled: false,
                MatchesUpdated: 0,
                MatchesFinished: 0,
                HadInProgressMatch: false,
                NeedsDiscoveryPoll: false,
                SkipReason: "no_in_progress_matches");
        }

        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.AuthToken))
        {
            return new LiveMatchSyncResult(
                ApiCalled: false,
                MatchesUpdated: 0,
                MatchesFinished: 0,
                HadInProgressMatch: true,
                NeedsDiscoveryPoll: false,
                SkipReason: "disabled");
        }

        var now = DateTime.UtcNow;
        var updatedMatchIds = new List<Guid>();
        var finishedMatchIds = new List<Guid>();
        IReadOnlyList<FootballDataLiveMatchSnapshot> liveFeed;

        try
        {
            liveFeed = await _footballDataClient.GetLiveMatchesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nie udało się pobrać meczów LIVE z football-data.org.");
            return new LiveMatchSyncResult(
                ApiCalled: false,
                MatchesUpdated: 0,
                MatchesFinished: 0,
                HadInProgressMatch: true,
                NeedsDiscoveryPoll: StillNeedsDiscovery(liveMatches),
                SkipReason: "api_error");
        }

        var feedByMatchId = liveFeed.ToDictionary(s => s.MatchId);

        foreach (var match in liveMatches)
        {
            FootballDataLiveMatchSnapshot? snapshot = null;

            if (match.LiveApiFixtureId is int fixtureId && feedByMatchId.TryGetValue(fixtureId, out var byId))
                snapshot = byId;
            else
                snapshot = liveFeed.FirstOrDefault(s => LiveMatchDiscoveryRules.MatchesTeams(s, match));

            if (snapshot is not null)
            {
                RememberApiIds(match, snapshot);

                if (ApplySnapshot(match, snapshot, now))
                    updatedMatchIds.Add(match.Id);

                continue;
            }

            if (match.LiveApiFixtureId is int trackedId)
            {
                FootballDataLiveMatchSnapshot? detail = null;
                try
                {
                    detail = await _footballDataClient.GetMatchAsync(trackedId, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Nie udało się pobrać meczu {FixtureId} z football-data.org.", trackedId);
                }

                if (detail is not null)
                {
                    RememberApiIds(match, detail);

                    if (ApplySnapshot(match, detail, now))
                        updatedMatchIds.Add(match.Id);

                    if (FootballDataMatchStatusRules.IsLive(detail.Status))
                    {
                        _logger.LogDebug(
                            "Mecz {MatchId} (fixture {FixtureId}) poza feedem LIVE, ale detail = {Status} — bez kończenia.",
                            match.Id,
                            trackedId,
                            detail.Status);
                        continue;
                    }

                    if (FootballDataMatchStatusRules.IsMatchFinished(detail.Status))
                        continue;
                }

                if (ShouldAutoFinish(match.KickOffUtc, now))
                {
                    MarkMatchFinished(match, now);
                    updatedMatchIds.Add(match.Id);
                    _logger.LogInformation(
                        "Mecz {MatchId} zakończony awaryjnie po {Hours}h od kickoffu (brak w feedzie LIVE, detail: {Status}).",
                        match.Id,
                        MatchLifecycleRules.LiveDuration.TotalHours,
                        detail?.Status ?? "brak");
                }
                else
                {
                    _logger.LogDebug(
                        "Mecz {MatchId} (fixture {FixtureId}) poza feedem LIVE — czekamy (detail: {Status}).",
                        match.Id,
                        trackedId,
                        detail?.Status ?? "brak");
                }

                continue;
            }

            if (NeedsDiscovery(match))
            {
                match.LiveApiDiscoveryAttempts++;
                _logger.LogDebug(
                    "football-data.org: brak meczu {Home}–{Away} w feedzie LIVE (próba {Attempt}/{Max}).",
                    match.HomeTeam.Name,
                    match.AwayTeam.Name,
                    match.LiveApiDiscoveryAttempts,
                    _options.DiscoveryMaxAttempts);
            }

            if (ShouldAutoFinish(match.KickOffUtc, now))
            {
                MarkMatchFinished(match, now);
                updatedMatchIds.Add(match.Id);
                _logger.LogInformation(
                    "Mecz {MatchId} ({Home}–{Away}) zakończony automatycznie po czasie regulaminowym.",
                    match.Id,
                    match.HomeTeam.Name,
                    match.AwayTeam.Name);
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

        return new LiveMatchSyncResult(
            ApiCalled: true,
            MatchesUpdated: updatedMatchIds.Distinct().Count(),
            MatchesFinished: finishedMatchIds.Distinct().Count(),
            HadInProgressMatch: true,
            NeedsDiscoveryPoll: StillNeedsDiscovery(liveMatches),
            SkipReason: null);
    }

    private bool NeedsDiscovery(Match match) =>
        !match.LiveApiFixtureId.HasValue
        && match.LiveApiDiscoveryAttempts < _options.DiscoveryMaxAttempts;

    private bool StillNeedsDiscovery(IEnumerable<Match> liveMatches) =>
        liveMatches.Any(NeedsDiscovery);

    private static bool ShouldAutoFinish(DateTime kickOffUtc, DateTime now) =>
        now >= MatchLifecycleRules.EnsureUtc(kickOffUtc).Add(MatchLifecycleRules.LiveDuration);

    private static void RememberApiIds(Match match, FootballDataLiveMatchSnapshot snapshot)
    {
        match.LiveApiFixtureId = snapshot.MatchId;

        if (!match.HomeTeam.LiveApiId.HasValue)
            match.HomeTeam.LiveApiId = snapshot.HomeTeamId;

        if (!match.AwayTeam.LiveApiId.HasValue)
            match.AwayTeam.LiveApiId = snapshot.AwayTeamId;
    }

    private bool ApplySnapshot(Match match, FootballDataLiveMatchSnapshot snapshot, DateTime now)
    {
        var previousHome = match.HomeScore;
        var previousAway = match.AwayScore;
        var previousPhase = match.ClockPhase;
        var previousBaseMinute = match.ClockBaseMinute;
        var previousUseManualClock = match.UseManualClock;
        var previousLiveKickOff = match.LiveApiKickOffUtc;
        var previousClockStarted = match.ClockStartedUtc;
        var finished = FootballDataMatchStatusRules.IsMatchFinished(snapshot.Status);
        var phase = FootballDataMatchStatusRules.MapPhase(snapshot.Status, match.ClockPhase);

        if (snapshot.HomeGoals is int home)
            match.HomeScore = home;

        if (snapshot.AwayGoals is int away)
            match.AwayScore = away;

        match.ClockPhase = phase;
        ApplyClockFromSnapshot(match, snapshot, phase, previousPhase, now);
        match.UpdatedAt = now;

        if (finished)
        {
            MarkMatchFinished(match, now);
            _logger.LogInformation(
                "Mecz {MatchId} zakończony wg football-data.org (api {ApiId}, status {Status}, wynik {Home}-{Away}).",
                match.Id,
                snapshot.MatchId,
                snapshot.Status,
                match.HomeScore,
                match.AwayScore);
        }

        return previousHome != match.HomeScore
            || previousAway != match.AwayScore
            || previousPhase != match.ClockPhase
            || previousBaseMinute != match.ClockBaseMinute
            || previousUseManualClock != match.UseManualClock
            || previousLiveKickOff != match.LiveApiKickOffUtc
            || previousClockStarted != match.ClockStartedUtc
            || finished;
    }

    private static void ApplyClockFromSnapshot(
        Match match,
        FootballDataLiveMatchSnapshot snapshot,
        MatchClockPhase phase,
        MatchClockPhase previousPhase,
        DateTime now)
    {
        if (snapshot.UtcDate is DateTime utcDate)
            match.LiveApiKickOffUtc = MatchLifecycleRules.EnsureUtc(utcDate);

        switch (phase)
        {
            case MatchClockPhase.HalfTime:
                match.UseManualClock = true;
                match.ClockBaseMinute = 45;
                match.ClockStartedUtc = null;
                break;

            case MatchClockPhase.FullTime:
                match.UseManualClock = true;
                match.ClockStartedUtc = null;
                match.LiveApiKickOffUtc = null;
                break;

            case MatchClockPhase.SecondHalf:
                match.UseManualClock = true;
                if (previousPhase == MatchClockPhase.HalfTime)
                {
                    match.ClockBaseMinute = 46;
                    match.ClockStartedUtc = now;
                }
                else if (match.ClockStartedUtc is null)
                {
                    // Sync w trakcie 2. połowy bez widzianej przerwy — szacunek od kickoffu API.
                    var kickOff = match.LiveApiKickOffUtc ?? match.KickOffUtc;
                    match.ClockBaseMinute = 46;
                    match.ClockStartedUtc = MatchLifecycleRules.EnsureUtc(kickOff).AddMinutes(60);
                }
                break;

            default:
                match.UseManualClock = false;
                match.ClockBaseMinute = 0;
                match.ClockStartedUtc = null;
                break;
        }
    }

    private static void MarkMatchFinished(Match match, DateTime now)
    {
        match.Status = MatchStatus.Finished;
        match.ClockPhase = MatchClockPhase.FullTime;
        match.ClockStartedUtc = null;
        match.LiveApiKickOffUtc = null;
        match.UseManualClock = true;
        match.UpdatedAt = now;
    }
}
