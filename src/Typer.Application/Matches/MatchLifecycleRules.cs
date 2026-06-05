using Typer.Domain.Enums;

namespace Typer.Application.Matches;

public static class MatchLifecycleRules
{
    public static readonly TimeSpan LiveDuration = TimeSpan.FromHours(2);

    public static DateTime EnsureUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    public static MatchStatus GetEffectiveStatus(MatchStatus stored, DateTime kickOffUtc, DateTime? utcNow = null)
    {
        if (stored is MatchStatus.Finished or MatchStatus.Cancelled)
            return stored;

        var now = utcNow ?? DateTime.UtcNow;
        var kickOff = EnsureUtc(kickOffUtc);

        if (now >= kickOff.Add(LiveDuration))
            return MatchStatus.Finished;

        if (now >= kickOff)
            return MatchStatus.InProgress;

        return stored;
    }

    public static string GetEffectiveStatusName(MatchStatus stored, DateTime kickOffUtc, DateTime? utcNow = null) =>
        GetEffectiveStatus(stored, kickOffUtc, utcNow).ToString();

    public static string GetPredictionStatus(MatchStatus stored, DateTime kickOffUtc, DateTime? utcNow = null)
    {
        var effective = GetEffectiveStatus(stored, kickOffUtc, utcNow);
        return effective switch
        {
            MatchStatus.Finished  => "Scored",
            MatchStatus.Cancelled => "Locked",
            MatchStatus.Scheduled => "Open",
            _ => "Locked"
        };
    }

    public static bool IsBeforeKickOff(DateTime kickOffUtc, DateTime? utcNow = null) =>
        (utcNow ?? DateTime.UtcNow) < EnsureUtc(kickOffUtc);
}
