using Typer.Domain.Enums;

namespace Typer.Application.Matches;

public static class MatchResultsOrderingRules
{
    public static IEnumerable<T> OrderForResultsPage<T>(
        IEnumerable<T> matches,
        Func<T, MatchStatus> getStoredStatus,
        Func<T, DateTime> getKickOffUtc,
        DateTime? utcNow = null) =>
        matches
            .OrderByDescending(m =>
                MatchLifecycleRules.GetEffectiveStatus(getStoredStatus(m), getKickOffUtc(m), utcNow)
                == MatchStatus.InProgress)
            .ThenByDescending(m => MatchLifecycleRules.EnsureUtc(getKickOffUtc(m)));

    public static bool IsLive(MatchStatus storedStatus, DateTime kickOffUtc, DateTime? utcNow = null) =>
        MatchLifecycleRules.GetEffectiveStatus(storedStatus, kickOffUtc, utcNow) == MatchStatus.InProgress;
}
