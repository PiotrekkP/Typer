using Typer.Domain.Enums;

namespace Typer.Infrastructure.Integrations.ApiFootball;

internal static class ApiFootballClockMapper
{
    public static MatchClockPhase MapPhase(string statusLong, bool finished)
    {
        if (finished || IsFinishedStatus(statusLong))
            return MatchClockPhase.FullTime;

        var normalized = statusLong.Trim();

        if (ContainsAny(normalized, "Half Time", "Halftime", "HT"))
            return MatchClockPhase.HalfTime;

        if (ContainsAny(normalized, "Second Half", "2nd Half", "2H"))
            return MatchClockPhase.SecondHalf;

        if (ContainsAny(normalized, "First Half", "1st Half", "1H"))
            return MatchClockPhase.FirstHalf;

        return MatchClockPhase.FirstHalf;
    }

    private static bool IsFinishedStatus(string statusLong) =>
        ContainsAny(
            statusLong,
            "Match Finished",
            "Full Time",
            "After Extra Time",
            "After Penalties",
            "FT",
            "AET",
            "PEN");

    private static bool ContainsAny(string value, params string[] needles)
    {
        foreach (var needle in needles)
        {
            if (value.Contains(needle, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
