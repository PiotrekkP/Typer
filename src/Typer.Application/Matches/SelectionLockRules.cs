namespace Typer.Application.Matches;

public static class SelectionLockRules
{
    public const string LockedMessage =
        "Wybór reprezentacji i zawodnika jest zablokowany od rozpoczęcia turnieju.";

    /// <summary>
    /// Selection stays open until the first scheduled match kick-off (UTC).
    /// When there are no matches, selection remains open.
    /// </summary>
    public static bool IsSelectionOpen(DateTime? firstKickOffUtc, DateTime? utcNow = null)
    {
        if (firstKickOffUtc is null)
            return true;

        return MatchLifecycleRules.IsBeforeKickOff(firstKickOffUtc.Value, utcNow);
    }
}
