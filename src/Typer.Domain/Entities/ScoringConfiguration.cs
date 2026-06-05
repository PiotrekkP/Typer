using Typer.Domain.Common;

namespace Typer.Domain.Entities;

/// <summary>
/// Konfigurowalne zasady punktacji. Tylko jedna konfiguracja może być aktywna.
/// Przygotowane pod przyszły panel admina.
/// </summary>
public class ScoringConfiguration : BaseEntity
{
    public required string Name { get; set; }
    public bool IsActive { get; set; }

    // ── Punkty za typ ────────────────────────────────────────
    /// <summary>Punkty za trafienie zwycięzcy (lub remisu).</summary>
    public int CorrectWinnerPoints { get; set; } = 2;

    /// <summary>Bonus na trafiony zwycięzca + dokładna różnica bramek.</summary>
    public int CorrectGoalDifferenceBonus { get; set; } = 1;

    /// <summary>Punkty za dokładny wynik (pełne 5).</summary>
    public int ExactScorePoints { get; set; } = 5;

    // ── Mnożniki i bonusy ────────────────────────────────────
    /// <summary>Mnożnik punktów za mecz ulubionej reprezentacji.</summary>
    public double FavoriteTeamMultiplier { get; set; } = 2.0;

    /// <summary>Premia za wygranie całych mistrzostw przez ulubioną drużynę.</summary>
    public int TournamentWinnerBonus { get; set; } = 20;

    /// <summary>Punkty za każdego gola ulubionego zawodnika.</summary>
    public int FavoritePlayerGoalBonus { get; set; } = 3;
}
