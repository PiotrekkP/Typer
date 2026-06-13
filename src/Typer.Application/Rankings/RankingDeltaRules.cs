using Typer.Application.Rankings.DTOs;

namespace Typer.Application.Rankings;

public static class RankingDeltaRules
{
    public static Dictionary<string, int> ComputeDeltas(
        IReadOnlyDictionary<string, int> baselinePoints,
        IReadOnlyList<RankingEntryDto> current)
    {
        var oldRanks = ComputeRanks(current, baselinePoints);
        var newRanks = ComputeRanks(current);
        var deltas = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var entry in current)
        {
            if (!oldRanks.TryGetValue(entry.UserId, out var oldPosition))
                continue;
            if (!newRanks.TryGetValue(entry.UserId, out var newPosition))
                continue;

            var delta = oldPosition - newPosition;
            if (delta != 0)
                deltas[entry.UserId] = delta;
        }

        return deltas;
    }

    public static Dictionary<string, int> ComputePointDeltas(
        IReadOnlyDictionary<string, int> baselinePoints,
        IReadOnlyList<RankingEntryDto> current)
    {
        var deltas = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var entry in current)
        {
            if (!baselinePoints.TryGetValue(entry.UserId, out var baseline))
                continue;

            var delta = entry.TotalPoints - baseline;
            if (delta != 0)
                deltas[entry.UserId] = delta;
        }

        return deltas;
    }

    public static Dictionary<string, int> ComputeRanks(
        IReadOnlyList<RankingEntryDto> entries,
        IReadOnlyDictionary<string, int> pointsByUser)
    {
        var ranks = new Dictionary<string, int>(StringComparer.Ordinal);
        var ordered = entries
            .Select(e => (e.UserId, e.DisplayName, Points: pointsByUser.GetValueOrDefault(e.UserId, e.TotalPoints)))
            .OrderByDescending(x => x.Points)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.UserId, StringComparer.Ordinal)
            .ToList();

        for (var i = 0; i < ordered.Count; i++)
            ranks[ordered[i].UserId] = i + 1;

        return ranks;
    }

    private static Dictionary<string, int> ComputeRanks(IReadOnlyList<RankingEntryDto> entries) =>
        ComputeRanks(entries, entries.ToDictionary(e => e.UserId, e => e.TotalPoints, StringComparer.Ordinal));
}
