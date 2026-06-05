using Typer.Domain.Common;

namespace Typer.Domain.Entities;

public class Prediction : BaseEntity
{
    public required string UserId { get; set; }

    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public int PredictedHomeScore { get; set; }
    public int PredictedAwayScore { get; set; }

    public int? PointsAwarded { get; set; }

    // Breakdown przechowywany per-predykcja — umożliwia poprawne cofnięcie punktów przy rescore
    public int? BasePoints { get; set; }
    public int? TeamBonusPoints { get; set; }
    public int? PlayerGoalPoints { get; set; }

    public DateTime SubmittedAt { get; set; }
}
