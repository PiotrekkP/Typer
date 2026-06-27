using Typer.Application.Matches;

namespace Typer.Application.Tests.Matches;

public class DisplayTimeRulesTests
{
    [Fact]
    public void FromPolandTime_AndToPolandTime_RoundTrip()
    {
        var local = new DateTime(2026, 6, 20, 18, 30, 0, DateTimeKind.Unspecified);
        var utc = DisplayTimeRules.FromPolandTime(local);
        var back = DisplayTimeRules.ToPolandTime(utc);

        Assert.Equal(local, back);
    }

    [Fact]
    public void FromPolandTime_SummerTime_OffsetIsTwoHours()
    {
        var local = new DateTime(2026, 6, 20, 18, 0, 0, DateTimeKind.Unspecified);
        var utc = DisplayTimeRules.FromPolandTime(local);

        Assert.Equal(new DateTime(2026, 6, 20, 16, 0, 0, DateTimeKind.Utc), utc);
    }
}
