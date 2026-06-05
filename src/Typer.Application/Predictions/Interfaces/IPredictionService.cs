namespace Typer.Application.Predictions.Interfaces;

using Typer.Application.Common.Models;
using Typer.Application.Predictions.DTOs;

public interface IPredictionService
{
    Task<IReadOnlyList<MatchDto>> GetUpcomingMatchesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PredictionDto>> GetUserPredictionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<PredictionDto>> SubmitAsync(string userId, SubmitPredictionRequest request, CancellationToken cancellationToken = default);
}
