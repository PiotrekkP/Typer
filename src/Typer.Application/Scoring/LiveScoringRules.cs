namespace Typer.Application.Scoring;

public static class LiveScoringRules
{
    /// <summary>Interwał live przeliczania punktów (LiveScoringBackgroundService) i odświeżania rankingu.</summary>
    public static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);
}
