using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Typer.Application.Teams.DTOs;
using Typer.Application.Teams.Interfaces;

namespace Typer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetAll(CancellationToken cancellationToken)
    {
        var teams = await _teamService.GetAllAsync(cancellationToken);
        return Ok(teams);
    }

    [HttpPost("select")]
    [Authorize]
    public async Task<IActionResult> Select(SelectTeamRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _teamService.SelectTeamAsync(userId, request, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Error);
    }
}
