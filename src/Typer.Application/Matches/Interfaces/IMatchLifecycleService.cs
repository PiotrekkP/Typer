namespace Typer.Application.Matches.Interfaces;

public interface IMatchLifecycleService
{
    /// <summary>
    /// Scheduled → InProgress at kickoff (wynik 0:0); InProgress → Finished after live window.
    /// </summary>
    /// <returns>Liczba meczów, które właśnie przeszły w InProgress.</returns>
    Task<int> AdvanceStatusesAsync(CancellationToken cancellationToken = default);
}
