using Typer.Application.Common.Models;
using Typer.Application.Matches;
using Typer.Application.Matches.DTOs;

namespace Typer.Application.Matches.Interfaces;

public interface IMatchService
{
    Task<IReadOnlyList<RoundWithMatchesDto>> GetRoundsWithMatchesAsync(
        string? userId = null,
        MatchRoundsScope scope = MatchRoundsScope.Active,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktualizuje wynik i status meczu.
    /// Jeśli nowy status to Finished, automatycznie przelicza punkty za predykcje.
    /// </summary>
    Task<Result> UpdateMatchResultAsync(
        Guid matchId,
        UpdateMatchResultRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchDetailDto>> GetUpcomingMatchesAsync(
        int count = 5,
        string? userId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchDetailDto>> GetRecentFinishedMatchesAsync(
        int count = 5,
        string? userId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasInProgressMatchesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResultsMatchOptionDto>> GetResultsMatchOptionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchPredictionResultDto>> GetMatchPredictionResultsAsync(
        Guid matchId,
        CancellationToken cancellationToken = default);
}
