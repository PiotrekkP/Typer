namespace Typer.Application.Matches.Interfaces;

public interface ISelectionLockService
{
    Task<bool> IsSelectionOpenAsync(CancellationToken cancellationToken = default);

    /// <summary>UTC moment when selection closes (first tournament kick-off), or null if no matches exist.</summary>
    Task<DateTime?> GetSelectionLockUtcAsync(CancellationToken cancellationToken = default);
}
