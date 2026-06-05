using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Typer.Application.Common.Models;
using Typer.Application.Players.DTOs;
using Typer.Application.Players.Interfaces;
using Typer.Domain.Entities;
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
        var players = await PlayersQuery()
            .Where(p => p.TeamId == teamId)
            .OrderBy(p => p.JerseyNumber)
            .ToListAsync(cancellationToken);

        return players.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<PlayerDto>> GetMvpsAsync(CancellationToken cancellationToken = default)
    {
        var players = await PlayersQuery()
            .Where(p => p.IsMvp)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync(cancellationToken);

        return players.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<PlayerDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var term = query.Trim();
        if (term.Length < 2)
            return [];

        var pattern = $"%{term}%";

        var results = await PlayersQuery()
            .Where(p =>
                EF.Functions.ILike(p.FirstName, pattern) ||
                EF.Functions.ILike(p.LastName, pattern) ||
                EF.Functions.ILike(p.FirstName + " " + p.LastName, pattern) ||
                (p.Club != null && EF.Functions.ILike(p.Club, pattern)))
            .OrderByDescending(p => p.IsMvp)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (results.Count > 0)
            return results.Select(Map).ToList();

        // Bounded fallback when diacritics differ between query and stored names.
        var prefix = term.Length > 3 ? term[..3] : term;
        var prefixPattern = $"%{prefix}%";

        var candidates = await PlayersQuery()
            .Where(p =>
                EF.Functions.ILike(p.LastName, prefixPattern) ||
                EF.Functions.ILike(p.FirstName, prefixPattern))
            .OrderByDescending(p => p.IsMvp)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Take(200)
            .ToListAsync(cancellationToken);

        return candidates
            .Where(p => MatchesNormalized(p, term))
            .Take(50)
            .Select(Map)
            .ToList();
    }

    private static bool MatchesNormalized(Player player, string term)
    {
        var normalizedTerm = Normalize(term);
        return Normalize(player.FirstName).Contains(normalizedTerm) ||
               Normalize(player.LastName).Contains(normalizedTerm) ||
               Normalize($"{player.FirstName} {player.LastName}").Contains(normalizedTerm) ||
               (player.Club is not null && Normalize(player.Club).Contains(normalizedTerm));
    }

    private static string Normalize(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    public async Task<Result> SelectPlayerAsync(string userId, SelectPlayerRequest request, CancellationToken cancellationToken = default)
    {
        var playerExists = await _context.Players
            .AnyAsync(p => p.Id == request.PlayerId, cancellationToken);

        if (!playerExists)
            return Result.Failure("Wybrany zawodnik nie istnieje.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is null)
            return Result.Failure("Profil użytkownika nie został znaleziony.");

        profile.SelectedPlayerId = request.PlayerId;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private IQueryable<Player> PlayersQuery()
        => _context.Players
            .AsNoTracking()
            .Include(p => p.Team);

    private static PlayerDto Map(Player p)
        => new(
            p.Id,
            p.TeamId,
            p.FirstName,
            p.LastName,
            p.JerseyNumber,
            p.Position,
            p.Club,
            p.IsMvp,
            p.Team?.Name,
            p.Team?.FlagUrl);
}
