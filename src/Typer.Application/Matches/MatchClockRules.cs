using Typer.Domain.Enums;

namespace Typer.Application.Matches;

public static class MatchClockRules
{
    public static string GetLiveMinuteDisplay(
        bool useManualClock,
        MatchClockPhase phase,
        DateTime? clockStartedUtc,
        int clockBaseMinute,
        DateTime kickOffUtc,
        DateTime? utcNow = null)
    {
        if (useManualClock)
            return GetManualClockDisplay(phase, clockStartedUtc, clockBaseMinute, utcNow);

        return GetKickOffClockDisplay(kickOffUtc, utcNow);
    }

    public static int GetRunningMinute(
        MatchClockPhase phase,
        DateTime? clockStartedUtc,
        int clockBaseMinute,
        DateTime? utcNow = null)
    {
        if (phase is not (MatchClockPhase.FirstHalf or MatchClockPhase.SecondHalf) || clockStartedUtc is null)
            return clockBaseMinute;

        var elapsed = (int)((utcNow ?? DateTime.UtcNow) - MatchLifecycleRules.EnsureUtc(clockStartedUtc.Value)).TotalMinutes;
        return Math.Max(clockBaseMinute, clockBaseMinute + elapsed);
    }

    private static string GetManualClockDisplay(
        MatchClockPhase phase,
        DateTime? clockStartedUtc,
        int clockBaseMinute,
        DateTime? utcNow)
    {
        return phase switch
        {
            MatchClockPhase.PreMatch => "0'",
            MatchClockPhase.HalfTime => "45+'",
            MatchClockPhase.FullTime => "FT",
            MatchClockPhase.FirstHalf => FormatMinute(GetRunningMinute(phase, clockStartedUtc, clockBaseMinute, utcNow), maxRegular: 45),
            MatchClockPhase.SecondHalf => FormatMinute(GetRunningMinute(phase, clockStartedUtc, clockBaseMinute, utcNow), maxRegular: 90),
            _ => "0'"
        };
    }

    private static string GetKickOffClockDisplay(DateTime kickOffUtc, DateTime? utcNow)
    {
        var elapsed = (int)((utcNow ?? DateTime.UtcNow) - MatchLifecycleRules.EnsureUtc(kickOffUtc)).TotalMinutes;
        if (elapsed <= 0) return "0'";
        if (elapsed <= 45) return $"{elapsed}'";
        if (elapsed <= 60) return "45+'";
        var half2 = elapsed - 15;
        return half2 <= 90 ? $"{half2}'" : "90+'";
    }

    private static string FormatMinute(int minute, int maxRegular)
    {
        if (minute <= maxRegular) return $"{minute}'";
        return $"{maxRegular}+'";
    }
}
