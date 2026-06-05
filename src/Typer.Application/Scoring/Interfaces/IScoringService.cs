using Typer.Application.Common.Models;

namespace Typer.Application.Scoring.Interfaces;

public interface IScoringService
{
    /// <summary>
    /// Przelicza punkty za predykcje meczu (mecz musi być Finished z wynikiem).
    /// Uwzględnia mnożnik dla ulubionej drużyny i bonusy za gole ulubionego zawodnika.
    /// </summary>
    Task<Result> ScoreMatchAsync(Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cofa stare punkty za mecz i przelicza je od nowa na podstawie aktualnych
    /// strzelców bramek i aktualnej konfiguracji. Przydatne gdy strzelcy zostali
    /// poprawieni po pierwotnym naliczeniu punktów.
    /// </summary>
    Task<Result> RescoreMatchAsync(Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Przyznaje premię 20 pkt wszystkim użytkownikom, których ulubiona drużyna
    /// wygrała cały turniej.
    /// </summary>
    Task<Result> ScoreTournamentWinnerAsync(Guid winnerTeamId, CancellationToken cancellationToken = default);
}
