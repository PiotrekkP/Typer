using Typer.Application.Matches.DTOs;
using Typer.Domain.Enums;

namespace Typer.Application.Matches;

public static class LiveUiRefreshRules
{
    public static TimeSpan GetRefreshInterval(bool isDevelopment) =>
        isDevelopment ? TimeSpan.FromSeconds(15) : TimeSpan.FromSeconds(30);

    public static bool HasLiveMatches(IEnumerable<RoundWithMatchesDto>? rounds) =>
        rounds?.Any(r => r.Matches.Any(IsLiveMatch)) ?? false;

    public static bool HasLiveMatches(IEnumerable<MatchDetailDto>? matches) =>
        matches?.Any(IsLiveMatch) ?? false;

    public static bool IsLiveMatch(MatchDetailDto match) =>
        string.Equals(match.Status, nameof(MatchStatus.InProgress), StringComparison.OrdinalIgnoreCase)
        || MatchLifecycleRules.GetEffectiveStatus(ParseStatus(match.Status), match.KickOffUtc)
            == MatchStatus.InProgress;

    private static MatchStatus ParseStatus(string status) =>
        Enum.TryParse<MatchStatus>(status, out var parsed) ? parsed : MatchStatus.Scheduled;
}
