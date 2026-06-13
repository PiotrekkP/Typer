using Typer.Domain.Common;

namespace Typer.Domain.Entities;

public class UserProfile : BaseEntity
{
    public required string UserId { get; set; }
    public required string DisplayName { get; set; }

    public Guid? SelectedTeamId { get; set; }
    public Team? SelectedTeam { get; set; }

    public Guid? SelectedPlayerId { get; set; }
    public Player? SelectedPlayer { get; set; }

    public int TotalPoints { get; set; }

    // Podział punktów według kategorii
    public int PredictionPoints { get; set; }
    public int TeamBonusPoints { get; set; }
    public int PlayerGoalPoints { get; set; }
    public int TournamentWinnerPoints { get; set; }

    public bool VipUser { get; set; }
}
