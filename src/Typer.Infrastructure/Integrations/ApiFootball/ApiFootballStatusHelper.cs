namespace Typer.Infrastructure.Integrations.ApiFootball;

internal static class ApiFootballStatusHelper
{
    private static readonly HashSet<string> FinishedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "FT", "AET", "PEN", "AWD", "WO"
    };

    public static bool IsFinished(string statusShort) =>
        FinishedStatuses.Contains(statusShort);

    public static bool IsHalftime(string statusShort) =>
        string.Equals(statusShort, "HT", StringComparison.OrdinalIgnoreCase);
}
