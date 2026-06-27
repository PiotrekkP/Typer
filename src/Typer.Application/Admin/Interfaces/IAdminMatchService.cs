using Typer.Application.Admin.DTOs;
using Typer.Application.Common.Models;
using Typer.Application.Matches.DTOs;

namespace Typer.Application.Admin.Interfaces;

public interface IAdminMatchService
{
    Task<IReadOnlyList<AdminMatchListItemDto>> GetMatchesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminRoundOptionDto>> GetRoundOptionsAsync(CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateMatchAsync(CreateMatchRequest request, CancellationToken cancellationToken = default);

    Task<AdminMatchDetailDto?> GetMatchAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<Result> UpdateResultAsync(Guid matchId, UpdateMatchResultRequest request, CancellationToken cancellationToken = default);

    Task<Result> RescoreAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<Result> AddGoalScorerAsync(Guid matchId, AddGoalScorerRequest request, CancellationToken cancellationToken = default);

    Task<Result> RemoveGoalScorerAsync(Guid goalScorerId, CancellationToken cancellationToken = default);

    Task<Result> StartFirstHalfAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<Result> StartHalfTimeAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<Result> StartSecondHalfAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<Result> FinishMatchAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<Result> SetMinuteAsync(Guid matchId, SetMatchMinuteRequest request, CancellationToken cancellationToken = default);
}
