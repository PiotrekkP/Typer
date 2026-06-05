using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Typer.Application.Rankings.DTOs;
using Typer.Application.Rankings.Interfaces;

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
        var ranking = await _rankingService.GetLeaderboardAsync(cancellationToken);
        return Ok(ranking);
    }
}
