using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Typer.Application.Scoring;
using Typer.Application.Scoring.Interfaces;

namespace Typer.Infrastructure.Background;

public class LiveScoringBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = LiveScoringRules.RefreshInterval;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiveScoringBackgroundService> _logger;

    public LiveScoringBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<LiveScoringBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LiveScoringBackgroundService uruchomiony (interwał {Interval} min).", Interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var scoring = scope.ServiceProvider.GetRequiredService<IScoringService>();
                await scoring.UpdateLiveScoresForInProgressMatchesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas live przeliczania punktów.");
            }
        }
    }
}
