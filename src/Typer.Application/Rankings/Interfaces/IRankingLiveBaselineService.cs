namespace Typer.Application.Rankings.Interfaces;

/// <summary>
/// Utrzymuje snapshot rankingu z momentu rozpoczęcia sesji meczów na żywo.
/// </summary>
public interface IRankingLiveBaselineService
{
    /// <summary>
    /// Przy pierwszym meczu InProgress zapisuje baseline; po zakończeniu ostatniego meczu go resetuje.
    /// </summary>
    Task SyncWithLiveMatchesAsync(CancellationToken cancellationToken = default);
}
