using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Typer.Application.Rankings.DTOs;
using Typer.Application.Rankings.Interfaces;
using Typer.Infrastructure.Services;

namespace Typer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RankingsController : ControllerBase
{
    private readonly IRankingService _rankingService;

    public RankingsController(IRankingService rankingService)
    {
        _rankingService = rankingService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<RankingEntryDto>>> GetLeaderboard(CancellationToken cancellationToken)
    {
        var ranking = await _rankingService.GetLeaderboardAsync(cancellationToken: cancellationToken);
        return Ok(ranking);
    }

    [HttpGet("vip")]
    [Authorize(Roles = VipRoleSeeder.VipRoleName)]
    public async Task<ActionResult<IReadOnlyList<RankingEntryDto>>> GetVipLeaderboard(CancellationToken cancellationToken)
    {
        var ranking = await _rankingService.GetLeaderboardAsync(vipOnly: true, cancellationToken: cancellationToken);
        return Ok(ranking);
    }
}
