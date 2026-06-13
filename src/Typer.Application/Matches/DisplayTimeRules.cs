namespace Typer.Application.Matches;

public static class DisplayTimeRules
{
    private static readonly Lazy<TimeZoneInfo> PolandTimeZone = new(ResolvePolandTimeZone);

    public static DateTime ToPolandTime(DateTime utc)
    {
        var utcValue = MatchLifecycleRules.EnsureUtc(utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcValue, PolandTimeZone.Value);
    }

    public static string FormatPolandTime(DateTime utc, string format)
        => ToPolandTime(utc).ToString(format);

    private static TimeZoneInfo ResolvePolandTimeZone()
    {
        string[] ids = ["Europe/Warsaw", "Central European Standard Time"];

        foreach (var id in ids)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new InvalidOperationException(
            "Nie znaleziono strefy Europe/Warsaw — zainstaluj pakiet tzdata w obrazie Docker.");
    }
}
