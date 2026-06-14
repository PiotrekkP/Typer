namespace Typer.Infrastructure.Options;

public class ApiFootballOptions
{
    public const string SectionName = "ApiFootball";

    public bool Enabled { get; set; }

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://v3.football.api-sports.io/";

    /// <summary>Liga API-Sports do discovery (MŚ = 1).</summary>
    public int LeagueId { get; set; } = 1;

    /// <summary>Interwał odpytywania odds/live?fixture= po discovery (domyślnie 4 min).</summary>
    public int PollIntervalMinutes { get; set; } = 4;

    /// <summary>Interwał discovery (odds/live feed) — co minutę, max DiscoveryMaxAttempts razy.</summary>
    public int DiscoveryPollIntervalMinutes { get; set; } = 1;

    /// <summary>Maks. liczba zapytań discovery na mecz.</summary>
    public int DiscoveryMaxAttempts { get; set; } = 5;

    /// <summary>Szukaj meczów z elapsed &lt;= tej wartości (minuty gry w API).</summary>
    public int DiscoveryMaxElapsedMinutes { get; set; } = 5;
}
