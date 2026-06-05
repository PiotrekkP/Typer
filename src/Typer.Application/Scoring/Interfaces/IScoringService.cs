using Typer.Application.Common.Models;

namespace Typer.Application.Scoring.Interfaces;

public interface IScoringService
{
    /// <summary>
    /// Przelicza punkty za predykcje meczu (mecz musi być Finished z wynikiem).
    /// </summary>
    Task<Result> ScoreMatchAsync(Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Przelicza punkty na podstawie bieżącego wyniku i strzelców (InProgress lub Finished).
    /// Aktualizuje profile deltą względem poprzedniego stanu predykcji.
    /// </summary>
    Task<Result> RecalculateMatchScoresAsync(Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Przelicza punkty dla wszystkich trwających meczów z ustawionym wynikiem.
    /// </summary>
    Task UpdateLiveScoresForInProgressMatchesAsync(CancellationToken cancellationToken = default);

    Task<Result> RescoreMatchAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<Result> ScoreTournamentWinnerAsync(Guid winnerTeamId, CancellationToken cancellationToken = default);
}
