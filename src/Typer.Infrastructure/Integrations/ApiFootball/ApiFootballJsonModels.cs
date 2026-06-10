using System.Text.Json.Serialization;

namespace Typer.Infrastructure.Integrations.ApiFootball;

internal sealed class ApiFootballResponse<T>
{
    [JsonPropertyName("response")]
    public List<T>? Response { get; set; }
}

internal sealed class ApiFootballFixtureItem
{
    [JsonPropertyName("fixture")]
    public ApiFootballFixtureInfo? Fixture { get; set; }

    [JsonPropertyName("teams")]
    public ApiFootballTeams? Teams { get; set; }

    [JsonPropertyName("goals")]
    public ApiFootballGoals? Goals { get; set; }
}

internal sealed class ApiFootballFixtureInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("status")]
    public ApiFootballStatus? Status { get; set; }
}

internal sealed class ApiFootballStatus
{
    [JsonPropertyName("short")]
    public string? Short { get; set; }
}

internal sealed class ApiFootballTeams
{
    [JsonPropertyName("home")]
    public ApiFootballTeamRef? Home { get; set; }

    [JsonPropertyName("away")]
    public ApiFootballTeamRef? Away { get; set; }
}

internal sealed class ApiFootballTeamRef
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

internal sealed class ApiFootballGoals
{
    [JsonPropertyName("home")]
    public int? Home { get; set; }

    [JsonPropertyName("away")]
    public int? Away { get; set; }
}

internal sealed class ApiFootballEventItem
{
    [JsonPropertyName("time")]
    public ApiFootballEventTime? Time { get; set; }

    [JsonPropertyName("team")]
    public ApiFootballTeamRef? Team { get; set; }

    [JsonPropertyName("player")]
    public ApiFootballPerson? Player { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}

internal sealed class ApiFootballEventTime
{
    [JsonPropertyName("elapsed")]
    public int Elapsed { get; set; }

    [JsonPropertyName("extra")]
    public int? Extra { get; set; }
}

internal sealed class ApiFootballPerson
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
