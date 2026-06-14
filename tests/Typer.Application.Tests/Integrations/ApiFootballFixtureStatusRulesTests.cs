using Typer.Application.Integrations.ApiFootball;
using Typer.Domain.Enums;

namespace Typer.Application.Tests.Integrations;

public class ApiFootballFixtureStatusRulesTests
{
    [Theory]
    [InlineData("Halftime", null)]
    [InlineData("Half Time", null)]
    [InlineData("Halftime", "HT")]
    public void MapPhase_Halftime_IsNotFullTime(string statusLong, string? statusShort)
    {
        Assert.Equal(MatchClockPhase.HalfTime, ApiFootballFixtureStatusRules.MapPhase(statusLong, statusShort));
        Assert.False(ApiFootballFixtureStatusRules.IsMatchFinished(statusLong, statusShort));
    }

    [Theory]
    [InlineData("Match Finished", "FT")]
    [InlineData("Full Time", "FT")]
    [InlineData("After Extra Time", "AET")]
    public void MapPhase_Finished_ReturnsFullTime(string statusLong, string statusShort)
    {
        Assert.Equal(MatchClockPhase.FullTime, ApiFootballFixtureStatusRules.MapPhase(statusLong, statusShort));
        Assert.True(ApiFootballFixtureStatusRules.IsMatchFinished(statusLong, statusShort));
    }

    [Fact]
    public void IsMatchFinished_Halftime_DoesNotMatchFtSubstring()
    {
        Assert.False(ApiFootballFixtureStatusRules.IsMatchFinished("Halftime", null));
    }
}
