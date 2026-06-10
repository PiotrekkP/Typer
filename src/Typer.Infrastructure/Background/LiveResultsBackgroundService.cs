using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typer.Application.Integrations.ApiFootball;
using Typer.Infrastructure.Options;

namespace Typer.Infrastructure.Background;

/// <summary>
/// Pobiera wyniki live z API-Football co <see cref="ApiFootballOptions.PollIntervalMinutes"/> minut,
/// ale tylko gdy w bazie są mecze InProgress z mapowaniem na API.
/// Gdy brak trwających meczów — krótszy interwał oczekiwania (1 min), bez zużywania limitu API.
/// </summary>
public sealed class LiveResultsBackgroundService : BackgroundService
{
    private static readonly TimeSpan IdleInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiveResultsBackgroundService> _logger;

    public LiveResultsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<LiveResultsBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LiveResultsBackgroundService uruchomiony.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan nextDelay = IdleInterval;

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var sync = scope.ServiceProvider.GetRequiredService<ILiveResultsSyncService>();
                var options = scope.ServiceProvider.GetRequiredService<IOptions<ApiFootballOptions>>().Value;

                var result = await sync.SyncAsync(stoppingToken);

                if (result.HadInProgressMatches && result.ApiCalled)
                {
                    var delayMinutes = result.NextPollDelayMinutes ?? options.PollIntervalMinutes;
                    nextDelay = TimeSpan.FromMinutes(Math.Max(1, delayMinutes));
                }
                else
                {
                    nextDelay = IdleInterval;
                }

                if (result.SkipReason == "no_api_mapping" && result.HadInProgressMatches)
                {
                    _logger.LogWarning(
                        "Trwają mecze bez mapowania ApiFootballFixtureId / ApiFootballTeamId — pominięto synchronizację live.");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd synchronizacji wyników live z API-Football.");
                nextDelay = IdleInterval;
            }

            try
            {
                await Task.Delay(nextDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
