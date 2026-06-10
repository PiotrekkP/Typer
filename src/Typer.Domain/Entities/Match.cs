using Typer.Domain.Common;
using Typer.Domain.Enums;

namespace Typer.Domain.Entities;

public class Match : BaseEntity
{
    public Guid SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public Guid? RoundId { get; set; }
    public Round? Round { get; set; }

    public Guid HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = null!;

    public Guid AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public DateTime KickOffUtc { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }

    /// <summary>Identyfikator meczu w API-Football (fixtures.id) do synchronizacji live.</summary>
    public int? ApiFootballFixtureId { get; set; }

    public ICollection<Prediction> Predictions { get; set; } = [];
    public ICollection<GoalScorer> GoalScorers { get; set; } = [];
}
