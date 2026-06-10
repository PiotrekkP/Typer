namespace Typer.Application.Integrations.ApiFootball;

public sealed record LiveResultsSyncResult(
    bool ApiCalled,
    int ApiCallsMade,
    int MatchesUpdated,
    bool HadInProgressMatches,
    string? SkipReason = null,
    /// <summary>Null = użyj domyślnego PollIntervalMinutes z konfiguracji.</summary>
    int? NextPollDelayMinutes = null);
