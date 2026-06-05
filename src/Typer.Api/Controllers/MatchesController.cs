using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Typer.Application.Matches.DTOs;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Scoring.Interfaces;

namespace Typer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;
    private readonly IScoringService _scoringService;

    public MatchesController(IMatchService matchService, IScoringService scoringService)
    {
        _matchService   = matchService;
        _scoringService = scoringService;
    }

    /// <summary>
    /// Zwraca wszystkie kolejki z meczami. Jeśli użytkownik jest zalogowany,
    /// dołącza jego typy i zdobyte punkty.
    /// </summary>
    [HttpGet("rounds")]
    public async Task<ActionResult<IReadOnlyList<RoundWithMatchesDto>>> GetRounds(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var rounds = await _matchService.GetRoundsWithMatchesAsync(userId, cancellationToken);
        return Ok(rounds);
    }

    /// <summary>
    /// Aktualizuje wynik i status meczu.
    /// Gdy status = 2 (Finished), punkty za predykcje są przyznawane automatycznie.
    /// </summary>
    [HttpPatch("{matchId:guid}/result")]
    [Authorize]
    public async Task<IActionResult> UpdateResult(
        Guid matchId,
        [FromBody] UpdateMatchResultRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _matchService.UpdateMatchResultAsync(matchId, request, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Wynik zaktualizowany." });
    }

    /// <summary>
    /// Cofa punkty przyznane za mecz i przelicza je od nowa.
    /// Używane gdy np. strzelcy bramek zostali skorygowani po naliczeniu punktów.
    /// </summary>
    [HttpPost("{matchId:guid}/rescore")]
    [Authorize]
    public async Task<IActionResult> Rescore(Guid matchId, CancellationToken cancellationToken)
    {
        var result = await _scoringService.RescoreMatchAsync(matchId, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Punkty przeliczone od nowa." });
    }
}
