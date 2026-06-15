namespace Typer.Application.Integrations.FootballData;

public interface ILiveMatchSyncService
{
    Task<LiveMatchSyncResult> SyncAsync(CancellationToken cancellationToken = default);
}

public sealed record LiveMatchSyncResult(
    bool ApiCalled,
    int MatchesUpdated,
    int MatchesFinished,
    bool HadInProgressMatch,
    bool NeedsDiscoveryPoll,
    string? SkipReason);
