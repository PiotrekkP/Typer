using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typer.Application.Integrations.ApiFootball;
using Typer.Infrastructure.Options;

namespace Typer.Infrastructure.Background;

/// <summary>
/// Discovery: odds/live (liga) co 1 min (max 5 prób). Po znalezieniu fixture: odds/live?fixture= co 4 min.
/// </summary>
public sealed class LiveOddsBackgroundService : BackgroundService
{
    private static readonly TimeSpan IdleInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiveOddsBackgroundService> _logger;

    public LiveOddsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<LiveOddsBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LiveOddsBackgroundService uruchomiony.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan nextDelay = IdleInterval;

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var sync = scope.ServiceProvider.GetRequiredService<ILiveOddsSyncService>();
                var options = scope.ServiceProvider.GetRequiredService<IOptions<ApiFootballOptions>>().Value;

                var result = await sync.SyncAsync(stoppingToken);

                if (result.HadInProgressMatch && result.ApiCalled)
                {
                    nextDelay = result.NeedsDiscoveryPoll
                        ? TimeSpan.FromMinutes(Math.Max(1, options.DiscoveryPollIntervalMinutes))
                        : TimeSpan.FromMinutes(Math.Max(1, options.PollIntervalMinutes));
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd synchronizacji odds/live.");
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
