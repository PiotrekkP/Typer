using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typer.Application.Integrations.FootballData;
using Typer.Application.Matches.Interfaces;
using Typer.Infrastructure.Options;

namespace Typer.Infrastructure.Background;

/// <summary>
/// Odpytuje football-data.org tylko gdy w bazie są mecze InProgress.
/// </summary>
public sealed class LiveMatchBackgroundService : BackgroundService
{
    private static readonly TimeSpan IdleWhenNoLiveMatches = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiveMatchBackgroundService> _logger;

    public LiveMatchBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<LiveMatchBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LiveMatchBackgroundService uruchomiony (football-data.org).");

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan nextDelay = IdleWhenNoLiveMatches;

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var matchService = scope.ServiceProvider.GetRequiredService<IMatchService>();

                if (!await matchService.HasInProgressMatchesAsync(stoppingToken))
                {
                    await Task.Delay(nextDelay, stoppingToken);
                    continue;
                }

                var sync = scope.ServiceProvider.GetRequiredService<ILiveMatchSyncService>();
                var options = scope.ServiceProvider.GetRequiredService<IOptions<FootballDataOptions>>().Value;
                var result = await sync.SyncAsync(stoppingToken);

                if (result.HadInProgressMatch && result.ApiCalled)
                {
                    var intervalSeconds = result.NeedsDiscoveryPoll
                        ? Math.Max(10, options.DiscoveryPollIntervalSeconds)
                        : Math.Max(10, options.PollIntervalSeconds);
                    nextDelay = TimeSpan.FromSeconds(intervalSeconds);
                }
                else if (result.HadInProgressMatch)
                {
                    nextDelay = TimeSpan.FromSeconds(Math.Max(10, options.PollIntervalSeconds));
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd synchronizacji wyników LIVE.");
                nextDelay = TimeSpan.FromSeconds(60);
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
