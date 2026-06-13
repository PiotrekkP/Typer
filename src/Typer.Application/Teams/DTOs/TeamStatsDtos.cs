using Typer.Application.Matches.DTOs;

namespace Typer.Application.Teams.DTOs;

public record GroupStandingRowDto(
    Guid TeamId,
    string Name,
    string Code,
    string? FlagUrl,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDiff,
    int Points);

public record GroupStandingsDto(
    string GroupName,
    IReadOnlyList<GroupStandingRowDto> Rows);

public record TeamStatsPageDto(
    TeamDto Team,
    GroupStandingsDto? GroupStandings,
    IReadOnlyList<MatchDetailDto> RecentMatches);
