using Microsoft.EntityFrameworkCore;
using Typer.Application.Common.Models;
using Typer.Application.Matches;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Teams.DTOs;
using Typer.Application.Teams.Interfaces;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ISelectionLockService _selectionLock;

    public TeamService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ISelectionLockService selectionLock)
    {
        _contextFactory = contextFactory;
        _selectionLock = selectionLock;
    }

    public async Task<IReadOnlyList<TeamDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Teams
            .OrderBy(t => t.GroupName)
            .ThenBy(t => t.Name)
            .Select(t => new TeamDto(t.Id, t.Name, t.Code, t.FlagUrl, t.GroupName))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result> SelectTeamAsync(string userId, SelectTeamRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _selectionLock.IsSelectionOpenAsync(cancellationToken))
            return Result.Failure(SelectionLockRules.LockedMessage);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var teamExists = await context.Teams.AnyAsync(t => t.Id == request.TeamId, cancellationToken);
        if (!teamExists)
            return Result.Failure("Wybrana reprezentacja nie istnieje.");

        var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is null)
            return Result.Failure("Profil użytkownika nie został znaleziony.");

        profile.SelectedTeamId = request.TeamId;
        profile.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
