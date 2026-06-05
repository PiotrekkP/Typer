using Microsoft.EntityFrameworkCore;
using Typer.Application.Common.Models;
using Typer.Application.Players.DTOs;
using Typer.Application.Players.Interfaces;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class PlayerService : IPlayerService
{
    private readonly ApplicationDbContext _context;

    public PlayerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PlayerDto>> GetByTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.Players
            .Where(p => p.TeamId == teamId)
            .OrderBy(p => p.JerseyNumber)
            .Select(p => new PlayerDto(p.Id, p.TeamId, p.FirstName, p.LastName, p.JerseyNumber, p.Position, p.Club))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result> SelectPlayerAsync(string userId, SelectPlayerRequest request, CancellationToken cancellationToken = default)
    {
        var player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PlayerId, cancellationToken);

        if (player is null)
        {
            return Result.Failure("Wybrany zawodnik nie istnieje.");
        }

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure("Profil użytkownika nie został znaleziony.");
        }

        if (profile.SelectedTeamId != player.TeamId)
        {
            return Result.Failure("Zawodnik musi należeć do wybranej reprezentacji.");
        }

        profile.SelectedPlayerId = request.PlayerId;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
