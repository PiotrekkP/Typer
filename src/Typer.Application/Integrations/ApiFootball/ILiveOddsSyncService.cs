namespace Typer.Application.Integrations.ApiFootball;

public interface ILiveOddsSyncService
{
    Task<LiveOddsSyncResult> SyncAsync(CancellationToken cancellationToken = default);
}

public sealed record LiveOddsSyncResult(
    bool ApiCalled,
    int MatchesUpdated,
    int MatchesFinished,
    bool HadInProgressMatch,
    bool NeedsDiscoveryPoll,
    string? SkipReason);
