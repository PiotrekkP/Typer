namespace Typer.Infrastructure.Options;

public class ApiFootballOptions
{
    public const string SectionName = "ApiFootball";

    public bool Enabled { get; set; }

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://v3.football.api-sports.io/";

    /// <summary>
    /// Interwał między zapytaniami live=all w trakcie gry (domyślnie 5 min).
    /// Mecz trwa wall-clock ~110–115 min (90 min + ~15 przerwa + ~5–10 dogrywki),
    /// więc jeden mecz ≈ 22–23 zapytania live; 4 równoległe mecze ≈ 22–23 łącznie (jedno live=all).
    /// </summary>
    public int PollIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Wydłużony interwał w przerwie (status HT) — wynik się nie zmienia, oszczędza limit API.
    /// </summary>
    public int HalftimePollIntervalMinutes { get; set; } = 10;
}
