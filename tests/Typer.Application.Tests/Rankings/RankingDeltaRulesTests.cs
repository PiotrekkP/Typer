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
    public void ComputePointDeltas_ReturnsGainSinceBaseline()
    {
        var baseline = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 8
        };

        var current = new List<RankingEntryDto>
        {
            Entry("a", 10, 1),
            Entry("b", 11, 2)
        };

        var deltas = RankingDeltaRules.ComputePointDeltas(baseline, current);

        Assert.Equal(3, deltas["b"]);
        Assert.False(deltas.ContainsKey("a"));
    }

    [Fact]
    public void ComputePointDeltas_OmitsUsersNotInBaseline()
    {
        var baseline = new Dictionary<string, int> { ["a"] = 10 };
        var current = new List<RankingEntryDto> { Entry("b", 5, 1) };

        var deltas = RankingDeltaRules.ComputePointDeltas(baseline, current);

        Assert.Empty(deltas);
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
