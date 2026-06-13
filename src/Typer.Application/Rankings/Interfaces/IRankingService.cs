namespace Typer.Application.Rankings.Interfaces;

using Typer.Application.Rankings.DTOs;

public interface IRankingService
{
    Task<IReadOnlyList<RankingEntryDto>> GetLeaderboardAsync(
        bool vipOnly = false,
        CancellationToken cancellationToken = default);

    Task<LeaderboardSnapshotDto> GetLeaderboardSnapshotAsync(
        bool vipOnly = false,
        CancellationToken cancellationToken = default);
}
