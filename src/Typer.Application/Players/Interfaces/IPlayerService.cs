namespace Typer.Application.Players.Interfaces;

using Typer.Application.Common.Models;
using Typer.Application.Players.DTOs;

public interface IPlayerService
{
    Task<IReadOnlyList<PlayerDto>> GetByTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<Result> SelectPlayerAsync(string userId, SelectPlayerRequest request, CancellationToken cancellationToken = default);
}
