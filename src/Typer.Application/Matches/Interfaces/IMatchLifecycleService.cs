namespace Typer.Application.Matches.Interfaces;

public interface IMatchLifecycleService
{
    /// <summary>
    /// Scheduled → InProgress at kickoff (wynik 0:0); InProgress → Finished after live window.
    /// </summary>
    Task AdvanceStatusesAsync(CancellationToken cancellationToken = default);
}
