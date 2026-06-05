using Typer.Domain.Common;

namespace Typer.Domain.Entities;

public class Season : BaseEntity
{
    public required string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Match> Matches { get; set; } = [];
}
