namespace Typer.Infrastructure.Integrations.FootballData;

internal sealed class FootballDataMatchesResponse
{
    public List<FootballDataMatchItem>? Matches { get; set; }
}

internal sealed class FootballDataMatchItem
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public int? Minute { get; set; }
    public int? InjuryTime { get; set; }
    public DateTime? UtcDate { get; set; }
    public DateTime? LastUpdated { get; set; }
    public FootballDataTeamSide? HomeTeam { get; set; }
    public FootballDataTeamSide? AwayTeam { get; set; }
    public FootballDataScore? Score { get; set; }
}

internal sealed class FootballDataTeamSide
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Tla { get; set; }
}

internal sealed class FootballDataScore
{
    public FootballDataScoreLine? FullTime { get; set; }
}

internal sealed class FootballDataScoreLine
{
    public int? Home { get; set; }
    public int? Away { get; set; }
}
