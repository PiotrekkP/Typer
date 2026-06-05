using Microsoft.EntityFrameworkCore;
using Typer.Application.Common.Models;
using Typer.Application.Teams.DTOs;
using Typer.Application.Teams.Interfaces;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly ApplicationDbContext _context;

    public TeamService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TeamDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .OrderBy(t => t.GroupName)
            .ThenBy(t => t.Name)
            .Select(t => new TeamDto(t.Id, t.Name, t.Code, t.FlagUrl, t.GroupName))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result> SelectTeamAsync(string userId, SelectTeamRequest request, CancellationToken cancellationToken = default)
    {
        var teamExists = await _context.Teams.AnyAsync(t => t.Id == request.TeamId, cancellationToken);
        if (!teamExists)
        {
            return Result.Failure("Wybrana reprezentacja nie istnieje.");
        }

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure("Profil użytkownika nie został znaleziony.");
        }

        profile.SelectedTeamId = request.TeamId;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
