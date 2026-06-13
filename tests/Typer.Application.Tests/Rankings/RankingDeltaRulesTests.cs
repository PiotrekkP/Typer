using Typer.Application.Rankings;
using Typer.Application.Rankings.DTOs;

namespace Typer.Application.Tests.Rankings;

public class RankingDeltaRulesTests
{
    [Fact]
    public void ComputeDeltas_ReturnsPositiveWhenUserMovedUp()
    {
        var baseline = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 8
        };

        var current = new List<RankingEntryDto>
        {
            Entry("b", 12, 1),
            Entry("a", 10, 2)
        };

        var deltas = RankingDeltaRules.ComputeDeltas(baseline, current);

        Assert.Equal(1, deltas["b"]);
    }

    [Fact]
    public void ComputeDeltas_OmitsUsersWithNoPositionChange()
    {
        var baseline = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 8
        };

        var current = new List<RankingEntryDto>
        {
            Entry("a", 10, 1),
            Entry("b", 8, 2)
        };

        var deltas = RankingDeltaRules.ComputeDeltas(baseline, current);

        Assert.Empty(deltas);
    }

    private static RankingEntryDto Entry(string userId, int points, int position) =>
        new(position, userId, userId, null, null, null, null, points, 0, 0, 0, 0);
}
