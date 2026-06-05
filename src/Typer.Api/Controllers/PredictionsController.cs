using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Typer.Application.Predictions.DTOs;
using Typer.Application.Predictions.Interfaces;

namespace Typer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionService _predictionService;

    public PredictionsController(IPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    [HttpGet("matches")]
    public async Task<ActionResult<IReadOnlyList<MatchDto>>> GetMatches(CancellationToken cancellationToken)
    {
        var matches = await _predictionService.GetUpcomingMatchesAsync(cancellationToken);
        return Ok(matches);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<PredictionDto>>> GetMyPredictions(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var predictions = await _predictionService.GetUserPredictionsAsync(userId, cancellationToken);
        return Ok(predictions);
    }

    [HttpPost]
    public async Task<ActionResult<PredictionDto>> Submit(SubmitPredictionRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _predictionService.SubmitAsync(userId, request, cancellationToken);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }
}
