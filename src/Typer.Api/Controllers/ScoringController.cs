using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Typer.Application.Scoring.Interfaces;

namespace Typer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScoringController : ControllerBase
{
    private readonly IScoringService _scoring;

    public ScoringController(IScoringService scoring)
    {
        _scoring = scoring;
    }

    /// <summary>
    /// Przelicza i zapisuje punkty za wszystkie predykcje danego meczu.
    /// Mecz musi być oznaczony jako Finished z uzupełnionym wynikiem.
    /// </summary>
    [HttpPost("matches/{matchId:guid}/calculate")]
    public async Task<IActionResult> ScoreMatch(Guid matchId, CancellationToken ct)
    {
        var result = await _scoring.ScoreMatchAsync(matchId, ct);

        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Punkty zostały przyznane." });
    }

    /// <summary>
    /// Przyznaje premię 20 pkt wszystkim użytkownikom, których ulubiona drużyna
    /// wygrała turniej.
    /// </summary>
    [HttpPost("tournament-winner/{teamId:guid}")]
    public async Task<IActionResult> ScoreTournamentWinner(Guid teamId, CancellationToken ct)
    {
        var result = await _scoring.ScoreTournamentWinnerAsync(teamId, ct);

        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Premia za mistrzostwo przyznana." });
    }
}
