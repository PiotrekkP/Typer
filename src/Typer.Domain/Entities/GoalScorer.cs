using Typer.Domain.Common;

namespace Typer.Domain.Entities;

public class GoalScorer : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public Guid? PlayerId { get; set; }
    public Player? Player { get; set; }

    public required string PlayerName { get; set; }
    public bool IsHomeTeam { get; set; }
    public int Minute { get; set; }
    public bool IsOwnGoal { get; set; }
    public bool IsPenalty { get; set; }
}
