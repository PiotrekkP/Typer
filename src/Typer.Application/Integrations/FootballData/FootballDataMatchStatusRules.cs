using Typer.Domain.Enums;

namespace Typer.Application.Integrations.FootballData;

public static class FootballDataMatchStatusRules
{
    public static MatchClockPhase MapPhase(string status) => status.Trim().ToUpperInvariant() switch
    {
        "PAUSED" => MatchClockPhase.HalfTime,
        "FINISHED" or "AWARDED" => MatchClockPhase.FullTime,
        "IN_PLAY" => MatchClockPhase.SecondHalf,
        _ => MatchClockPhase.FirstHalf
    };

    public static bool IsLive(string status)
    {
        var normalized = status.Trim().ToUpperInvariant();
        return normalized is "IN_PLAY" or "PAUSED";
    }

    public static bool IsMatchFinished(string status) =>
        status.Trim().ToUpperInvariant() is "FINISHED" or "AWARDED";
}
