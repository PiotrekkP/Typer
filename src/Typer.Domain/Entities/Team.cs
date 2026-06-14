using Typer.Domain.Common;

namespace Typer.Domain.Entities;

public class Team : BaseEntity
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? FlagUrl { get; set; }
    public string? GroupName { get; set; }

    /// <summary>ID drużyny w API-Sports (odds/live, teams).</summary>
    public int? LiveApiId { get; set; }

    public ICollection<Player> Players { get; set; } = [];
    public ICollection<Match> HomeMatches { get; set; } = [];
    public ICollection<Match> AwayMatches { get; set; } = [];
}
