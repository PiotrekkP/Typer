using Typer.Application.Matches;

namespace Typer.Application.Tests;

public class SelectionLockRulesTests
{
    private static readonly DateTime FirstKickOff = new(2026, 6, 11, 19, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsSelectionOpen_WhenNoMatches_ReturnsTrue()
    {
        Assert.True(SelectionLockRules.IsSelectionOpen(null, FirstKickOff.AddHours(-1)));
    }

    [Fact]
    public void IsSelectionOpen_BeforeFirstKickOff_ReturnsTrue()
    {
        Assert.True(SelectionLockRules.IsSelectionOpen(FirstKickOff, FirstKickOff.AddMinutes(-1)));
    }

    [Fact]
    public void IsSelectionOpen_AtFirstKickOff_ReturnsFalse()
    {
        Assert.False(SelectionLockRules.IsSelectionOpen(FirstKickOff, FirstKickOff));
    }

    [Fact]
    public void IsSelectionOpen_AfterFirstKickOff_ReturnsFalse()
    {
        Assert.False(SelectionLockRules.IsSelectionOpen(FirstKickOff, FirstKickOff.AddHours(1)));
    }
}
