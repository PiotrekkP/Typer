using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Typer.Application.Rankings.Interfaces;
using Typer.Domain.Entities;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class RankingLiveBaselineService : IRankingLiveBaselineService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<RankingLiveBaselineService> _logger;

    public RankingLiveBaselineService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<RankingLiveBaselineService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SyncWithLiveMatchesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var inProgress = await context.Matches
            .AnyAsync(m => m.Status == MatchStatus.InProgress, cancellationToken);

        var baselines = await context.RankingLiveBaselines.ToListAsync(cancellationToken);
        var sessionActive = baselines.Any(b => b.IsActive);

        if (inProgress && !sessionActive)
        {
            await CaptureBaselinesAsync(context, baselines, cancellationToken);
            _logger.LogInformation("Zapisano baseline rankingu — rozpoczęto sesję meczów na żywo.");
            return;
        }

        if (!inProgress && sessionActive)
        {
            foreach (var baseline in baselines)
                baseline.IsActive = false;

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Zresetowano baseline rankingu — brak meczów na żywo.");
        }
    }

    private static async Task CaptureBaselinesAsync(
        ApplicationDbContext context,
        List<RankingLiveBaseline> existing,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        foreach (var vipOnly in new[] { false, true })
        {
            var points = await GetCurrentPointsAsync(context, vipOnly, cancellationToken);
            var row = existing.FirstOrDefault(b => b.VipOnly == vipOnly);

            if (row is null)
            {
                row = new RankingLiveBaseline
                {
                    Id = Guid.NewGuid(),
                    VipOnly = vipOnly,
                    CreatedAt = now
                };
                context.RankingLiveBaselines.Add(row);
            }

            row.PointsJson = JsonSerializer.Serialize(points);
            row.IsActive = true;
            row.CapturedAtUtc = now;
            row.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    internal static async Task<Dictionary<string, int>> GetCurrentPointsAsync(
        ApplicationDbContext context,
        bool vipOnly,
        CancellationToken cancellationToken)
    {
        var query = context.UserProfiles.AsQueryable();
        if (vipOnly)
            query = query.Where(p => p.VipUser);

        return await query.ToDictionaryAsync(
            p => p.UserId,
            p => p.TotalPoints,
            StringComparer.Ordinal,
            cancellationToken);
    }

    internal static Dictionary<string, int>? DeserializePoints(string pointsJson)
    {
        if (string.IsNullOrWhiteSpace(pointsJson))
            return null;

        return JsonSerializer.Deserialize<Dictionary<string, int>>(pointsJson);
    }
}
