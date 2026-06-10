namespace Typer.Application.Integrations.ApiFootball;

public interface ILiveResultsSyncService
{
    /// <summary>
    /// Pobiera wyniki live z API-Football dla trwających meczów (jedno zapytanie live=all).
    /// Dodatkowe zapytania events tylko przy zmianie wyniku lub zakończeniu meczu.
    /// </summary>
    Task<LiveResultsSyncResult> SyncAsync(CancellationToken cancellationToken = default);
}
