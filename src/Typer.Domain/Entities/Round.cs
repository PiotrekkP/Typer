using Typer.Domain.Common;

namespace Typer.Domain.Entities;

public class Round : BaseEntity
{
    public Guid SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public required string Name { get; set; }
    public int OrderNumber { get; set; }

    public ICollection<Match> Matches { get; set; } = [];
}
