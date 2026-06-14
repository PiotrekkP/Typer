using Typer.Domain.Enums;

namespace Typer.Application.Integrations.ApiFootball;

/// <summary>
/// Mapuje status fixture z API-Football (nie flagi odds/live: stopped/blocked/finished).
/// </summary>
public static class ApiFootballFixtureStatusRules
{
    public static MatchClockPhase MapPhase(string statusLong, string? statusShort = null)
    {
        var normalized = statusLong.Trim();

        if (IsHalfTime(normalized, statusShort))
            return MatchClockPhase.HalfTime;

        if (IsMatchFinished(normalized, statusShort))
            return MatchClockPhase.FullTime;

        if (ContainsAny(normalized, "Second Half", "2nd Half")
            || string.Equals(statusShort, "2H", StringComparison.OrdinalIgnoreCase))
            return MatchClockPhase.SecondHalf;

        if (ContainsAny(normalized, "First Half", "1st Half")
            || string.Equals(statusShort, "1H", StringComparison.OrdinalIgnoreCase))
            return MatchClockPhase.FirstHalf;

        return MatchClockPhase.FirstHalf;
    }

    public static bool IsMatchFinished(string statusLong, string? statusShort = null)
    {
        if (IsHalfTime(statusLong, statusShort))
            return false;

        if (string.Equals(statusShort, "FT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(statusShort, "AET", StringComparison.OrdinalIgnoreCase)
            || string.Equals(statusShort, "PEN", StringComparison.OrdinalIgnoreCase))
            return true;

        return ContainsAny(
            statusLong.Trim(),
            "Match Finished",
            "Full Time",
            "After Extra Time",
            "After Penalties");
    }

    private static bool IsHalfTime(string statusLong, string? statusShort) =>
        string.Equals(statusShort, "HT", StringComparison.OrdinalIgnoreCase)
        || ContainsAny(statusLong.Trim(), "Half Time", "Halftime");

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
