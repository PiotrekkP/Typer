using Typer.Application.Integrations.FootballData;
using Typer.Domain.Enums;

namespace Typer.Application.Tests.Integrations;

public class FootballDataMatchStatusRulesTests
{
    [Theory]
    [InlineData("PAUSED")]
    public void MapPhase_Paused_IsHalfTime(string status)
    {
        Assert.Equal(MatchClockPhase.HalfTime, FootballDataMatchStatusRules.MapPhase(status));
        Assert.False(FootballDataMatchStatusRules.IsMatchFinished(status));
        Assert.True(FootballDataMatchStatusRules.IsLive(status));
    }

    [Theory]
    [InlineData("IN_PLAY")]
    public void MapPhase_InPlay_IsNotFinished(string status)
    {
        Assert.False(FootballDataMatchStatusRules.IsMatchFinished(status));
        Assert.True(FootballDataMatchStatusRules.IsLive(status));
    }

    [Theory]
    [InlineData("FINISHED")]
    [InlineData("AWARDED")]
    public void IsMatchFinished_TerminalStatuses(string status)
    {
        Assert.Equal(MatchClockPhase.FullTime, FootballDataMatchStatusRules.MapPhase(status));
        Assert.True(FootballDataMatchStatusRules.IsMatchFinished(status));
        Assert.False(FootballDataMatchStatusRules.IsLive(status));
    }
}
