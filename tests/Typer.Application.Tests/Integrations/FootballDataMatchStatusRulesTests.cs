using Typer.Application.Integrations.FootballData;
using Typer.Application.Matches;
using Typer.Domain.Enums;

namespace Typer.Application.Tests.Integrations;

public class FootballDataMatchStatusRulesTests
{
    private static readonly DateTime KickOff = new(2026, 6, 16, 19, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("PAUSED")]
    public void MapPhase_Paused_IsHalfTime(string status)
    {
        Assert.Equal(MatchClockPhase.HalfTime, FootballDataMatchStatusRules.MapPhase(status));
        Assert.False(FootballDataMatchStatusRules.IsMatchFinished(status));
        Assert.True(FootballDataMatchStatusRules.IsLive(status));
    }

    [Fact]
    public void MapPhase_InPlay_BeforePause_IsFirstHalf()
    {
        Assert.Equal(
            MatchClockPhase.FirstHalf,
            FootballDataMatchStatusRules.MapPhase("IN_PLAY", MatchClockPhase.FirstHalf));
    }

    [Fact]
    public void MapPhase_InPlay_AfterPause_IsSecondHalf()
    {
        Assert.Equal(
            MatchClockPhase.SecondHalf,
            FootballDataMatchStatusRules.MapPhase("IN_PLAY", MatchClockPhase.HalfTime));

        Assert.Equal(
            MatchClockPhase.SecondHalf,
            FootballDataMatchStatusRules.MapPhase("IN_PLAY", MatchClockPhase.SecondHalf));
    }

    [Fact]
    public void MapPhase_InPlay_DoesNotInferSecondHalfFromKickOffElapsed()
    {
        // 70 min po kickoffu, ale API nadal IN_PLAY (np. długa 1. połowa) — bez PAUSED zostaje 1. połowa.
        Assert.Equal(
            MatchClockPhase.FirstHalf,
            FootballDataMatchStatusRules.MapPhase("IN_PLAY", MatchClockPhase.FirstHalf));
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

    [Fact]
    public void FirstHalfClock_UsesUtcDateElapsedMinutes()
    {
        var now = KickOff.AddMinutes(23);

        Assert.Equal(
            "23'",
            MatchClockRules.GetLiveMinuteDisplay(
                useManualClock: false,
                MatchClockPhase.FirstHalf,
                clockStartedUtc: null,
                clockBaseMinute: 0,
                kickOffUtc: KickOff,
                utcNow: now));
    }

    [Fact]
    public void SecondHalfClock_CountsFromAnchorAfterPause()
    {
        var secondHalfStart = KickOff.AddMinutes(62);
        var now = secondHalfStart.AddMinutes(12);

        Assert.Equal(
            "58'",
            MatchClockRules.GetLiveMinuteDisplay(
                useManualClock: true,
                MatchClockPhase.SecondHalf,
                clockStartedUtc: secondHalfStart,
                clockBaseMinute: 46,
                kickOffUtc: KickOff,
                utcNow: now));
    }

    [Fact]
    public void HalfTimeClock_Shows45Plus()
    {
        Assert.Equal(
            "45+'",
            MatchClockRules.GetLiveMinuteDisplay(
                useManualClock: true,
                MatchClockPhase.HalfTime,
                clockStartedUtc: null,
                clockBaseMinute: 45,
                kickOffUtc: KickOff));
    }
}
