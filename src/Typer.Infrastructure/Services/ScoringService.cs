using Microsoft.EntityFrameworkCore;
using Typer.Application.Common.Models;
using Typer.Application.Scoring.Interfaces;
using Typer.Domain.Entities;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class ScoringService : IScoringService
{
    private readonly ApplicationDbContext _context;

    public ScoringService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> ScoreMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var match = await _context.Matches
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        if (match.Status != MatchStatus.Finished)
            return Result.Failure("Mecz nie jest jeszcze zakończony.");

        return await RecalculateMatchScoresAsync(matchId, cancellationToken);
    }

    public async Task<Result> RecalculateMatchScoresAsync(
        Guid matchId,
        CancellationToken cancellationToken = default)
    {
        var match = await _context.Matches
            .Include(m => m.GoalScorers)
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        if (match.Status is not (MatchStatus.InProgress or MatchStatus.Finished))
            return Result.Failure("Punkty live można liczyć tylko dla meczów na żywo lub zakończonych.");

        if (match.HomeScore is null || match.AwayScore is null)
            return Result.Success();

        var predictions = await _context.Predictions
            .Where(p => p.MatchId == matchId)
            .ToListAsync(cancellationToken);

        if (predictions.Count == 0)
            return Result.Success();

        var config = await _context.ScoringConfigurations
            .FirstOrDefaultAsync(c => c.IsActive, cancellationToken);

        if (config is null)
            return Result.Failure("Brak aktywnej konfiguracji punktacji.");

        var userIds = predictions.Select(p => p.UserId).Distinct().ToList();
        var profiles = await _context.UserProfiles
            .Where(p => userIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, cancellationToken);

        var playerGoals = match.GoalScorers
            .Where(g => g.PlayerId.HasValue && !g.IsOwnGoal)
            .GroupBy(g => g.PlayerId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var prediction in predictions)
        {
            var (earned, basePoints, teamBonus, playerGoalBonus) =
                CalculatePoints(match, prediction, profiles, playerGoals, config);

            ApplyPointsDelta(prediction, profiles, earned, basePoints, teamBonus, playerGoalBonus);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task UpdateLiveScoresForInProgressMatchesAsync(CancellationToken cancellationToken = default)
    {
        var matchIds = await _context.Matches
            .AsNoTracking()
            .Where(m => m.Status == MatchStatus.InProgress
                        && m.HomeScore != null
                        && m.AwayScore != null)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        foreach (var matchId in matchIds)
            await RecalculateMatchScoresAsync(matchId, cancellationToken);
    }

    public async Task<Result> RescoreMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var match = await _context.Matches
            .Include(m => m.GoalScorers)
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

        if (match is null)
            return Result.Failure("Mecz nie istnieje.");

        if (match.Status != MatchStatus.Finished)
            return Result.Failure("Mecz nie jest zakończony — rescore możliwy tylko dla zakończonych meczów.");

        if (match.HomeScore is null || match.AwayScore is null)
            return Result.Failure("Wynik meczu nie jest uzupełniony.");

        var predictions = await _context.Predictions
            .Where(p => p.MatchId == matchId)
            .ToListAsync(cancellationToken);

        if (predictions.Count == 0)
            return Result.Success();

        var userIds = predictions.Select(p => p.UserId).Distinct().ToList();
        var profiles = await _context.UserProfiles
            .Where(p => userIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, cancellationToken);

        foreach (var pred in predictions.Where(p => p.PointsAwarded.HasValue))
        {
            if (!profiles.TryGetValue(pred.UserId, out var profile)) continue;
            profile.TotalPoints      -= pred.PointsAwarded!.Value;
            profile.PredictionPoints -= pred.BasePoints     ?? 0;
            profile.TeamBonusPoints  -= pred.TeamBonusPoints ?? 0;
            profile.PlayerGoalPoints -= pred.PlayerGoalPoints ?? 0;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var pred in predictions)
        {
            pred.PointsAwarded    = null;
            pred.BasePoints       = null;
            pred.TeamBonusPoints  = null;
            pred.PlayerGoalPoints = null;
            pred.UpdatedAt        = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await RecalculateMatchScoresAsync(matchId, cancellationToken);
    }

    public async Task<Result> ScoreTournamentWinnerAsync(
        Guid winnerTeamId,
        CancellationToken cancellationToken = default)
    {
        var teamExists = await _context.Teams
            .AnyAsync(t => t.Id == winnerTeamId, cancellationToken);

        if (!teamExists)
            return Result.Failure("Drużyna nie istnieje.");

        var config = await _context.ScoringConfigurations
            .FirstOrDefaultAsync(c => c.IsActive, cancellationToken);

        if (config is null)
            return Result.Failure("Brak aktywnej konfiguracji punktacji.");

        var profiles = await _context.UserProfiles
            .Where(p => p.SelectedTeamId == winnerTeamId)
            .ToListAsync(cancellationToken);

        foreach (var profile in profiles)
        {
            profile.TotalPoints            += config.TournamentWinnerBonus;
            profile.TournamentWinnerPoints += config.TournamentWinnerBonus;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static (int earned, int basePoints, int teamBonus, int playerGoalBonus) CalculatePoints(
        Domain.Entities.Match match,
        Prediction prediction,
        Dictionary<string, UserProfile> profiles,
        Dictionary<Guid, int> playerGoals,
        ScoringConfiguration config)
    {
        var basePoints = CalculateBasePoints(
            match.HomeScore!.Value, match.AwayScore!.Value,
            prediction.PredictedHomeScore, prediction.PredictedAwayScore,
            config);

        bool favoriteTeamPlaying = false;
        int playerGoalBonus = 0;

        if (profiles.TryGetValue(prediction.UserId, out var profile))
        {
            favoriteTeamPlaying =
                profile.SelectedTeamId is not null &&
                (profile.SelectedTeamId == match.HomeTeamId ||
                 profile.SelectedTeamId == match.AwayTeamId);

            if (profile.SelectedPlayerId.HasValue &&
                playerGoals.TryGetValue(profile.SelectedPlayerId.Value, out var goals))
            {
                playerGoalBonus = goals * config.FavoritePlayerGoalBonus;
            }
        }

        var multiplier  = favoriteTeamPlaying ? config.FavoriteTeamMultiplier : 1.0;
        var scaledBase  = (int)Math.Floor(basePoints * multiplier);
        var teamBonus   = scaledBase - basePoints;
        var earned      = scaledBase + playerGoalBonus;

        return (earned, basePoints, teamBonus, playerGoalBonus);
    }

    private static void ApplyPointsDelta(
        Prediction prediction,
        Dictionary<string, UserProfile> profiles,
        int earned, int basePoints, int teamBonus, int playerGoalBonus)
    {
        var deltaEarned  = earned - (prediction.PointsAwarded ?? 0);
        var deltaBase    = basePoints - (prediction.BasePoints ?? 0);
        var deltaTeam    = teamBonus - (prediction.TeamBonusPoints ?? 0);
        var deltaPlayer  = playerGoalBonus - (prediction.PlayerGoalPoints ?? 0);

        if (deltaEarned == 0 && deltaBase == 0 && deltaTeam == 0 && deltaPlayer == 0)
            return;

        prediction.PointsAwarded    = earned;
        prediction.BasePoints       = basePoints;
        prediction.TeamBonusPoints  = teamBonus;
        prediction.PlayerGoalPoints = playerGoalBonus;
        prediction.UpdatedAt        = DateTime.UtcNow;

        if (profiles.TryGetValue(prediction.UserId, out var profile))
        {
            profile.TotalPoints      += deltaEarned;
            profile.PredictionPoints += deltaBase;
            profile.TeamBonusPoints  += deltaTeam;
            profile.PlayerGoalPoints += deltaPlayer;
            profile.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static int CalculateBasePoints(
        int actualHome, int actualAway,
        int predHome, int predAway,
        ScoringConfiguration config)
    {
        if (predHome == actualHome && predAway == actualAway)
            return config.ExactScorePoints;

        var actualSign = Math.Sign(actualHome - actualAway);
        var predSign   = Math.Sign(predHome   - predAway);

        if (actualSign != predSign)
            return 0;

        if ((actualHome - actualAway) == (predHome - predAway))
            return config.CorrectWinnerPoints + config.CorrectGoalDifferenceBonus;

        return config.CorrectWinnerPoints;
    }
}
