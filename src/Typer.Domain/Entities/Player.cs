using Typer.Domain.Common;

namespace Typer.Domain.Entities;

public class Player : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int JerseyNumber { get; set; }
    public string? Position { get; set; }

    public string? Club { get; set; }

    public bool IsMvp { get; set; }

    public string? PhotoUrl { get; set; }

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
}
