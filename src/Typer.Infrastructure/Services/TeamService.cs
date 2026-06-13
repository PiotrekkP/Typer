using Microsoft.EntityFrameworkCore;
using Typer.Application.Common.Models;
using Typer.Application.Matches;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Teams;
using Typer.Application.Teams.DTOs;
using Typer.Application.Teams.Interfaces;
using Typer.Domain.Enums;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ISelectionLockService _selectionLock;
    private readonly IMatchService _matchService;

    public TeamService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ISelectionLockService selectionLock,
        IMatchService matchService)
    {
        _contextFactory = contextFactory;
        _selectionLock = selectionLock;
        _matchService = matchService;
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

    public async Task<TeamStatsPageDto?> GetTeamStatsPageAsync(
        Guid teamId,
        string? userId = null,
        int recentMatchCount = 10,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var team = await context.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

        if (team is null)
            return null;

        var teamDto = new TeamDto(team.Id, team.Name, team.Code, team.FlagUrl, team.GroupName);
        GroupStandingsDto? groupStandings = null;

        if (!string.IsNullOrWhiteSpace(team.GroupName))
        {
            var groupTeams = await context.Teams
                .AsNoTracking()
                .Where(t => t.GroupName == team.GroupName)
                .OrderBy(t => t.Name)
                .Select(t => new TeamDto(t.Id, t.Name, t.Code, t.FlagUrl, t.GroupName))
                .ToListAsync(cancellationToken);

            var teamIds = groupTeams.Select(t => t.Id).ToHashSet();

            var finishedGroupMatches = await context.Matches
                .AsNoTracking()
                .Include(m => m.Round)
                .Where(m => m.Status == MatchStatus.Finished
                            && teamIds.Contains(m.HomeTeamId)
                            && teamIds.Contains(m.AwayTeamId))
                .ToListAsync(cancellationToken);

            var groupResults = finishedGroupMatches
                .Where(m => GroupStandingsRules.IsGroupStageRound(m.Round?.OrderNumber, m.Round?.Name)
                            && m.HomeScore.HasValue
                            && m.AwayScore.HasValue)
                .Select(m => new GroupStandingsRules.FinishedGroupMatch(
                    m.HomeTeamId,
                    m.AwayTeamId,
                    m.HomeScore!.Value,
                    m.AwayScore!.Value))
                .ToList();

            groupStandings = new GroupStandingsDto(
                team.GroupName,
                GroupStandingsRules.Calculate(groupTeams, groupResults));
        }

        var recentMatches = await _matchService.GetTeamRecentMatchesAsync(
            teamId,
            recentMatchCount,
            userId,
            cancellationToken);

        return new TeamStatsPageDto(teamDto, groupStandings, recentMatches);
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
