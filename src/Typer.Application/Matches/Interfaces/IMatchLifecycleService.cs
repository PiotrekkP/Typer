namespace Typer.Application.Matches.Interfaces;

public interface IMatchLifecycleService
{
    /// <summary>
    /// Scheduled → InProgress at kickoff; InProgress → Finished after live window.
    /// </summary>
    Task AdvanceStatusesAsync(CancellationToken cancellationToken = default);
}
