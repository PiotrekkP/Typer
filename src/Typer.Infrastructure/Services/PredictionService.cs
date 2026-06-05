using Microsoft.EntityFrameworkCore;
using Typer.Application.Common.Models;
using Typer.Application.Matches;
using Typer.Application.Predictions.DTOs;
using Typer.Application.Predictions.Interfaces;
using Typer.Domain.Entities;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class PredictionService : IPredictionService
{
    private readonly ApplicationDbContext _context;

    public PredictionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MatchDto>> GetUpcomingMatchesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.Status == MatchStatus.Scheduled || m.Status == MatchStatus.InProgress)
            .OrderBy(m => m.KickOffUtc)
            .Select(m => new MatchDto(
                m.Id,
                m.HomeTeamId,
                m.HomeTeam.Name,
                m.AwayTeamId,
                m.AwayTeam.Name,
                m.KickOffUtc,
                m.Status.ToString(),
                m.HomeScore,
                m.AwayScore))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PredictionDto>> GetUserPredictionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Predictions
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.SubmittedAt)
            .Select(p => new PredictionDto(
                p.Id,
                p.MatchId,
                p.PredictedHomeScore,
                p.PredictedAwayScore,
                p.PointsAwarded,
                p.SubmittedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<PredictionDto>> SubmitAsync(string userId, SubmitPredictionRequest request, CancellationToken cancellationToken = default)
    {
        var match = await _context.Matches.FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);
        if (match is null)
        {
            return Result<PredictionDto>.Failure("Mecz nie istnieje.");
        }

        if (match.Status != MatchStatus.Scheduled)
        {
            return Result<PredictionDto>.Failure("Typowanie na ten mecz jest zamknięte.");
        }

        if (!MatchLifecycleRules.IsBeforeKickOff(match.KickOffUtc))
        {
            return Result<PredictionDto>.Failure("Nie można typować meczu, który już się rozpoczął.");
        }

        var existing = await _context.Predictions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == request.MatchId, cancellationToken);

        if (existing is not null)
        {
            existing.PredictedHomeScore = request.PredictedHomeScore;
            existing.PredictedAwayScore = request.PredictedAwayScore;
            existing.SubmittedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<PredictionDto>.Success(new PredictionDto(
                existing.Id,
                existing.MatchId,
                existing.PredictedHomeScore,
                existing.PredictedAwayScore,
                existing.PointsAwarded,
                existing.SubmittedAt));
        }

        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MatchId = request.MatchId,
            PredictedHomeScore = request.PredictedHomeScore,
            PredictedAwayScore = request.PredictedAwayScore,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<PredictionDto>.Success(new PredictionDto(
            prediction.Id,
            prediction.MatchId,
            prediction.PredictedHomeScore,
            prediction.PredictedAwayScore,
            prediction.PointsAwarded,
            prediction.SubmittedAt));
    }
}
