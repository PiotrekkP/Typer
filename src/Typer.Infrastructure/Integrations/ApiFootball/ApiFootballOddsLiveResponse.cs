namespace Typer.Infrastructure.Integrations.ApiFootball;

internal sealed class ApiFootballResponse<T>
{
    public List<T>? Response { get; set; }
}

internal sealed class ApiFootballOddsLiveItem
{
    public ApiFootballOddsLiveFixture? Fixture { get; set; }
    public ApiFootballOddsLiveTeams? Teams { get; set; }
    public ApiFootballOddsLiveFlags? Status { get; set; }
}

internal sealed class ApiFootballOddsLiveFixture
{
    public int Id { get; set; }
    public ApiFootballFixtureStatus? Status { get; set; }
}

internal sealed class ApiFootballFixtureStatus
{
    public string? Long { get; set; }
    public string? Short { get; set; }
    public int? Elapsed { get; set; }
    public string? Seconds { get; set; }
}

internal sealed class ApiFootballOddsLiveTeams
{
    public ApiFootballOddsLiveTeamSide? Home { get; set; }
    public ApiFootballOddsLiveTeamSide? Away { get; set; }
}

internal sealed class ApiFootballOddsLiveTeamSide
{
    public int Id { get; set; }
    public int? Goals { get; set; }
}

internal sealed class ApiFootballOddsLiveFlags
{
    public bool Stopped { get; set; }
    public bool Blocked { get; set; }
    public bool Finished { get; set; }
}
