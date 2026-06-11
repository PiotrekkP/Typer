using Microsoft.EntityFrameworkCore;
using Typer.Application.Matches;
using Typer.Application.Matches.Interfaces;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class SelectionLockService : ISelectionLockService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public SelectionLockService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<DateTime?> GetSelectionLockUtcAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Matches
            .AsNoTracking()
            .Where(m => m.Status != MatchStatus.Cancelled)
            .MinAsync(m => (DateTime?)m.KickOffUtc, cancellationToken);
    }

    public async Task<bool> IsSelectionOpenAsync(CancellationToken cancellationToken = default)
    {
        var firstKickOff = await GetSelectionLockUtcAsync(cancellationToken);
        return SelectionLockRules.IsSelectionOpen(firstKickOff);
    }
}
