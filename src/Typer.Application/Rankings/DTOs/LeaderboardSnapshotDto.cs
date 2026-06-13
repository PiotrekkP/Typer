namespace Typer.Application.Rankings.DTOs;

public record LeaderboardSnapshotDto(
    IReadOnlyList<RankingEntryDto> Entries,
    IReadOnlyDictionary<string, int> PositionDeltas,
    bool LiveSessionActive);
