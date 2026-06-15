namespace Typer.Infrastructure.Options;

public class FootballDataOptions
{
    public const string SectionName = "FootballData";

    public bool Enabled { get; set; }

    /// <summary>Token z api.football-data.org — wysyłany w nagłówku X-Auth-Token.</summary>
    public string AuthToken { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.football-data.org/v4/";

    /// <summary>Interwał odpytywania feedu LIVE w sekundach (domyślnie 60 s = 1 zapytanie/min).</summary>
    public int PollIntervalSeconds { get; set; } = 60;

    /// <summary>Szybszy interwał, gdy mecz jeszcze nie ma dopasowania w feedzie (domyślnie 60 s).</summary>
    public int DiscoveryPollIntervalSeconds { get; set; } = 60;

    /// <summary>Maks. liczba prób dopasowania meczu z feedu LIVE.</summary>
    public int DiscoveryMaxAttempts { get; set; } = 10;
}
