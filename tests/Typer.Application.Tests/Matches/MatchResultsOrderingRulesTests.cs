using Typer.Application.Matches;
using Typer.Domain.Enums;

namespace Typer.Application.Tests.Matches;

public class MatchResultsOrderingRulesTests
{
    private static readonly DateTime Now = new(2026, 6, 20, 20, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void OrderForResultsPage_LiveBeforeFinished()
    {
        var liveKickOff = Now.AddHours(-1);
        var finishedKickOff = Now.AddHours(-2);

        var ordered = MatchResultsOrderingRules
            .OrderForResultsPage(
                new[]
                {
                    (Status: MatchStatus.Finished, KickOff: finishedKickOff),
                    (Status: MatchStatus.InProgress, KickOff: liveKickOff),
                },
                m => m.Status,
                m => m.KickOff,
                Now)
            .ToList();

        Assert.Equal(MatchStatus.InProgress, ordered[0].Status);
    }

    [Fact]
    public void OrderForResultsPage_FinishedByNewestKickOff()
    {
        var older = Now.AddDays(-2);
        var newer = Now.AddDays(-1);

        var ordered = MatchResultsOrderingRules
            .OrderForResultsPage(
                new[]
                {
                    (Status: MatchStatus.Finished, KickOff: older),
                    (Status: MatchStatus.Finished, KickOff: newer),
                },
                m => m.Status,
                m => m.KickOff,
                Now)
            .ToList();

        Assert.Equal(newer, ordered[0].KickOff);
    }
}
