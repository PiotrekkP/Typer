using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Typer.Application.Players.DTOs;
using Typer.Application.Players.Interfaces;

namespace Typer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayersController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet("team/{teamId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<PlayerDto>>> GetByTeam(Guid teamId, CancellationToken cancellationToken)
    {
        var players = await _playerService.GetByTeamAsync(teamId, cancellationToken);
        return Ok(players);
    }

    [HttpPost("select")]
    [Authorize]
    public async Task<IActionResult> Select(SelectPlayerRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _playerService.SelectPlayerAsync(userId, request, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Error);
    }
}
