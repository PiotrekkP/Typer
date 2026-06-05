namespace Typer.Application.Teams.DTOs;

public record TeamDto(Guid Id, string Name, string Code, string? FlagUrl, string? GroupName);

public record SelectTeamRequest(Guid TeamId);
