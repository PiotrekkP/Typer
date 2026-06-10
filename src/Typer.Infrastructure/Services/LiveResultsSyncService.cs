using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typer.Application.Integrations.ApiFootball;
using Typer.Application.Scoring.Interfaces;
using Typer.Domain.Entities;
using Typer.Domain.Enums;
using Typer.Infrastructure.Integrations.ApiFootball;
using Typer.Infrastructure.Options;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public sealed class LiveResultsSyncService : ILiveResultsSyncService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IApiFootballClient _apiFootballClient;
    private readonly IScoringService _scoringService;
    private readonly ApiFootballOptions _options;
    private readonly ILogger<LiveResultsSyncService> _logger;

    public LiveResultsSyncService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IApiFootballClient apiFootballClient,
        IScoringService scoringService,
        IOptions<ApiFootballOptions> options,
        ILogger<LiveResultsSyncService> logger)
    {
        _contextFactory = contextFactory;
        _apiFootballClient = apiFootballClient;
        _scoringService = scoringService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<LiveResultsSyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var inProgressMatches = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.GoalScorers)
            .Where(m => m.Status == MatchStatus.InProgress)
            .ToListAsync(cancellationToken);

        var mappableMatches = inProgressMatches
            .Where(CanMapToApiFootball)
            .ToList();

        if (mappableMatches.Count == 0)
        {
            return new LiveResultsSyncResult(
                ApiCalled: false,
                ApiCallsMade: 0,
                MatchesUpdated: 0,
                HadInProgressMatches: inProgressMatches.Count > 0,
                SkipReason: inProgressMatches.Count > 0 ? "no_api_mapping" : "no_in_progress_matches");
        }

        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new LiveResultsSyncResult(
                ApiCalled: false,
                ApiCallsMade: 0,
                MatchesUpdated: 0,
                HadInProgressMatches: true,
                SkipReason: "disabled");
        }

        var apiCalls = 0;
        var updatedMatchIds = new List<Guid>();

        IReadOnlyList<ApiFootballLiveFixture> liveFixtures;
        try
        {
            liveFixtures = await _apiFootballClient.GetLiveFixturesAsync(cancellationToken);
            apiCalls = 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nie udało się pobrać wyników live z API-Football.");
            return new LiveResultsSyncResult(
                ApiCalled: true,
                ApiCallsMade: apiCalls,
                MatchesUpdated: 0,
                HadInProgressMatches: true,
                SkipReason: "api_error");
        }

        var teamIds = mappableMatches
            .SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId })
            .Distinct()
            .ToList();

        var playersByTeam = await context.Players
            .Where(p => teamIds.Contains(p.TeamId))
            .ToListAsync(cancellationToken);

        var playersLookup = playersByTeam
            .GroupBy(p => p.TeamId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var trackedFixtures = new List<ApiFootballLiveFixture>();

        foreach (var match in mappableMatches)
        {
            var fixture = FindLiveFixture(match, liveFixtures);
            if (fixture is null)
                continue;

            trackedFixtures.Add(fixture);

            var previousHome = match.HomeScore;
            var previousAway = match.AwayScore;
            var previousGoalCount = TotalGoals(previousHome, previousAway);
            var newGoalCount = TotalGoals(fixture.HomeGoals, fixture.AwayGoals);
            var goalsIncreased = newGoalCount > previousGoalCount;
            var finished = ApiFootballStatusHelper.IsFinished(fixture.StatusShort);

            match.HomeScore = fixture.HomeGoals;
            match.AwayScore = fixture.AwayGoals;
            match.UpdatedAt = DateTime.UtcNow;

            var scorersNeedSync = goalsIncreased
                || (finished && match.GoalScorers.Count != newGoalCount);

            if (scorersNeedSync)
            {
                try
                {
                    var events = await _apiFootballClient.GetFixtureEventsAsync(fixture.FixtureId, cancellationToken);
                    apiCalls++;

                    ReplaceGoalScorers(
                        match,
                        events,
                        fixture.HomeTeamId,
                        match.HomeTeamId,
                        match.AwayTeamId,
                        playersLookup);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Nie udało się pobrać strzelców dla meczu {MatchId} (fixture {FixtureId}).",
                        match.Id,
                        fixture.FixtureId);
                }
            }

            if (finished)
            {
                match.Status = MatchStatus.Finished;
                _logger.LogInformation(
                    "Mecz {MatchId} zakończony wg API-Football (status {Status}, wynik {Home}-{Away}).",
                    match.Id,
                    fixture.StatusShort,
                    fixture.HomeGoals,
                    fixture.AwayGoals);
            }

            if (goalsIncreased || finished || previousHome != fixture.HomeGoals || previousAway != fixture.AwayGoals)
                updatedMatchIds.Add(match.Id);
        }

        if (updatedMatchIds.Count > 0)
            await context.SaveChangesAsync(cancellationToken);

        foreach (var matchId in updatedMatchIds.Distinct())
        {
            var scoringResult = await _scoringService.RecalculateMatchScoresAsync(matchId, cancellationToken);
            if (!scoringResult.Succeeded)
            {
                _logger.LogWarning(
                    "Nie udało się przeliczyć punktów live dla meczu {MatchId}: {Error}",
                    matchId,
                    scoringResult.Error);
            }
        }

        var allTrackedAtHalftime = trackedFixtures.Count > 0
            && trackedFixtures.All(f => ApiFootballStatusHelper.IsHalftime(f.StatusShort));

        int? nextPollDelay = allTrackedAtHalftime
            ? Math.Max(1, _options.HalftimePollIntervalMinutes)
            : null;

        if (allTrackedAtHalftime)
        {
            _logger.LogDebug(
                "Wszystkie śledzone mecze w przerwie (HT) — następne zapytanie za {Minutes} min.",
                nextPollDelay);
        }

        _logger.LogInformation(
            "Synchronizacja live: {Updated} meczów zaktualizowanych, {ApiCalls} zapytań API.",
            updatedMatchIds.Count,
            apiCalls);

        return new LiveResultsSyncResult(
            ApiCalled: true,
            ApiCallsMade: apiCalls,
            MatchesUpdated: updatedMatchIds.Count,
            HadInProgressMatches: true,
            NextPollDelayMinutes: nextPollDelay);
    }

    private static bool CanMapToApiFootball(Match match) =>
        match.ApiFootballFixtureId.HasValue
        || (match.HomeTeam.ApiFootballTeamId.HasValue && match.AwayTeam.ApiFootballTeamId.HasValue);

    private static ApiFootballLiveFixture? FindLiveFixture(
        Match match,
        IReadOnlyList<ApiFootballLiveFixture> liveFixtures)
    {
        if (match.ApiFootballFixtureId is int fixtureId)
            return liveFixtures.FirstOrDefault(f => f.FixtureId == fixtureId);

        var homeApiId = match.HomeTeam.ApiFootballTeamId!.Value;
        var awayApiId = match.AwayTeam.ApiFootballTeamId!.Value;

        return liveFixtures.FirstOrDefault(f =>
            f.HomeTeamId == homeApiId && f.AwayTeamId == awayApiId);
    }

    private static int TotalGoals(int? home, int? away) =>
        (home ?? 0) + (away ?? 0);

    private static void ReplaceGoalScorers(
        Match match,
        IReadOnlyList<ApiFootballFixtureEvent> events,
        int apiHomeTeamId,
        Guid dbHomeTeamId,
        Guid dbAwayTeamId,
        IReadOnlyDictionary<Guid, List<Player>> playersByTeam)
    {
        match.GoalScorers.Clear();

        var homePlayers = playersByTeam.GetValueOrDefault(dbHomeTeamId) ?? [];
        var awayPlayers = playersByTeam.GetValueOrDefault(dbAwayTeamId) ?? [];

        foreach (var goalEvent in events.OrderBy(e => e.Minute))
        {
            var isHomeTeam = goalEvent.TeamId == apiHomeTeamId;
            var detail = goalEvent.Detail ?? string.Empty;
            var isOwnGoal = detail.Contains("Own Goal", StringComparison.OrdinalIgnoreCase);
            var isPenalty = detail.Contains("Penalty", StringComparison.OrdinalIgnoreCase)
                && !isOwnGoal;

            var playerName = string.IsNullOrWhiteSpace(goalEvent.PlayerName)
                ? (isOwnGoal ? "Samobójczy" : "Nieznany")
                : goalEvent.PlayerName.Trim();

            var playerId = TryFindPlayerId(playerName, homePlayers, awayPlayers, isHomeTeam);

            match.GoalScorers.Add(new GoalScorer
            {
                Id = Guid.NewGuid(),
                MatchId = match.Id,
                PlayerId = playerId,
                PlayerName = playerName,
                IsHomeTeam = isHomeTeam,
                Minute = goalEvent.Minute,
                IsOwnGoal = isOwnGoal,
                IsPenalty = isPenalty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    private static Guid? TryFindPlayerId(
        string apiPlayerName,
        IReadOnlyList<Player> homePlayers,
        IReadOnlyList<Player> awayPlayers,
        bool isHomeTeam)
    {
        var pool = isHomeTeam ? homePlayers : awayPlayers;
        var normalizedApiName = NormalizeName(apiPlayerName);

        foreach (var player in pool)
        {
            var fullName = NormalizeName($"{player.FirstName} {player.LastName}");
            if (fullName == normalizedApiName)
                return player.Id;

            var lastName = NormalizeName(player.LastName);
            if (normalizedApiName.Contains(lastName, StringComparison.Ordinal)
                || lastName.Contains(normalizedApiName, StringComparison.Ordinal))
                return player.Id;
        }

        return null;
    }

    private static string NormalizeName(string name) =>
        name.Trim().ToLowerInvariant();
}
