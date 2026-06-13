namespace Typer.Application.Teams.Interfaces;

using Typer.Application.Common.Models;
using Typer.Application.Teams.DTOs;

public interface ITeamService
{
    Task<IReadOnlyList<TeamDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TeamStatsPageDto?> GetTeamStatsPageAsync(
        Guid teamId,
        string? userId = null,
        int recentMatchCount = 10,
        CancellationToken cancellationToken = default);

    Task<Result> SelectTeamAsync(string userId, SelectTeamRequest request, CancellationToken cancellationToken = default);
}
