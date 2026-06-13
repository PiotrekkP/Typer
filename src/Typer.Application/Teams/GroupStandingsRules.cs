using Typer.Application.Teams.DTOs;

namespace Typer.Application.Teams;

public static class GroupStandingsRules
{
    public record FinishedGroupMatch(Guid HomeTeamId, Guid AwayTeamId, int HomeScore, int AwayScore);

    public static IReadOnlyList<GroupStandingRowDto> Calculate(
        IReadOnlyList<TeamDto> teams,
        IReadOnlyList<FinishedGroupMatch> matches)
    {
        var stats = teams.ToDictionary(
            t => t.Id,
            t => new Accumulator(t));

        foreach (var match in matches)
        {
            if (!stats.TryGetValue(match.HomeTeamId, out var home)
                || !stats.TryGetValue(match.AwayTeamId, out var away))
                continue;

            home.Played++;
            away.Played++;
            home.GoalsFor += match.HomeScore;
            home.GoalsAgainst += match.AwayScore;
            away.GoalsFor += match.AwayScore;
            away.GoalsAgainst += match.HomeScore;

            if (match.HomeScore > match.AwayScore)
            {
                home.Won++;
                home.Points += 3;
                away.Lost++;
            }
            else if (match.HomeScore < match.AwayScore)
            {
                away.Won++;
                away.Points += 3;
                home.Lost++;
            }
            else
            {
                home.Drawn++;
                away.Drawn++;
                home.Points++;
                away.Points++;
            }
        }

        return stats.Values
            .Select(a => new GroupStandingRowDto(
                a.Team.Id,
                a.Team.Name,
                a.Team.Code,
                a.Team.FlagUrl,
                a.Played,
                a.Won,
                a.Drawn,
                a.Lost,
                a.GoalsFor,
                a.GoalsAgainst,
                a.GoalsFor - a.GoalsAgainst,
                a.Points))
            .OrderByDescending(r => r.Points)
            .ThenByDescending(r => r.GoalDiff)
            .ThenByDescending(r => r.GoalsFor)
            .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool IsGroupStageRound(int? orderNumber, string? roundName) =>
        orderNumber is <= 3
        || (roundName?.Contains("Faza grupowa", StringComparison.OrdinalIgnoreCase) ?? false);

    private sealed class Accumulator(TeamDto team)
    {
        public TeamDto Team { get; } = team;
        public int Played;
        public int Won;
        public int Drawn;
        public int Lost;
        public int GoalsFor;
        public int GoalsAgainst;
        public int Points;
    }
}
