using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Typer.Application.Admin.DTOs;
using Typer.Application.Admin.Interfaces;
using Typer.Application.Matches.DTOs;
using Typer.Infrastructure.Services;

namespace Typer.Api.Controllers;

[ApiController]
[Route("api/admin/matches")]
[Authorize(Roles = AdminRoleSeeder.AdminRoleName)]
public class AdminMatchesController : ControllerBase
{
    private readonly IAdminMatchService _adminMatchService;

    public AdminMatchesController(IAdminMatchService adminMatchService)
    {
        _adminMatchService = adminMatchService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminMatchListItemDto>>> GetMatches(CancellationToken cancellationToken)
    {
        var matches = await _adminMatchService.GetMatchesAsync(cancellationToken);
        return Ok(matches);
    }

    [HttpGet("{matchId:guid}")]
    public async Task<ActionResult<AdminMatchDetailDto>> GetMatch(Guid matchId, CancellationToken cancellationToken)
    {
        var match = await _adminMatchService.GetMatchAsync(matchId, cancellationToken);
        return match is null ? NotFound() : Ok(match);
    }

    [HttpPatch("{matchId:guid}/result")]
    public async Task<IActionResult> UpdateResult(
        Guid matchId,
        [FromBody] UpdateMatchResultRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adminMatchService.UpdateResultAsync(matchId, request, cancellationToken);
        return result.Succeeded ? Ok(new { message = "Wynik zaktualizowany." }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{matchId:guid}/rescore")]
    public async Task<IActionResult> Rescore(Guid matchId, CancellationToken cancellationToken)
    {
        var result = await _adminMatchService.RescoreAsync(matchId, cancellationToken);
        return result.Succeeded ? Ok(new { message = "Punkty przeliczone." }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{matchId:guid}/scorers")]
    public async Task<IActionResult> AddGoalScorer(
        Guid matchId,
        [FromBody] AddGoalScorerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adminMatchService.AddGoalScorerAsync(matchId, request, cancellationToken);
        return result.Succeeded ? Ok(new { message = "Strzelec dodany." }) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("scorers/{goalScorerId:guid}")]
    public async Task<IActionResult> RemoveGoalScorer(Guid goalScorerId, CancellationToken cancellationToken)
    {
        var result = await _adminMatchService.RemoveGoalScorerAsync(goalScorerId, cancellationToken);
        return result.Succeeded ? Ok(new { message = "Strzelec usunięty." }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{matchId:guid}/clock/first-half")]
    public async Task<IActionResult> StartFirstHalf(Guid matchId, CancellationToken cancellationToken)
    {
        var result = await _adminMatchService.StartFirstHalfAsync(matchId, cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{matchId:guid}/clock/half-time")]
    public async Task<IActionResult> StartHalfTime(Guid matchId, CancellationToken cancellationToken)
    {
        var result = await _adminMatchService.StartHalfTimeAsync(matchId, cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{matchId:guid}/clock/second-half")]
    public async Task<IActionResult> StartSecondHalf(Guid matchId, CancellationToken cancellationToken)
    {
        var result = await _adminMatchService.StartSecondHalfAsync(matchId, cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{matchId:guid}/clock/finish")]
    public async Task<IActionResult> FinishMatch(Guid matchId, CancellationToken cancellationToken)
    {
        var result = await _adminMatchService.FinishMatchAsync(matchId, cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{matchId:guid}/clock/minute")]
    public async Task<IActionResult> SetMinute(
        Guid matchId,
        [FromBody] SetMatchMinuteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adminMatchService.SetMinuteAsync(matchId, request, cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(new { error = result.Error });
    }
}
