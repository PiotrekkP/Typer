namespace Typer.Application.Players.DTOs;

public record PlayerDto(Guid Id, Guid TeamId, string FirstName, string LastName, int JerseyNumber, string? Position, string? Club);

public record SelectPlayerRequest(Guid PlayerId);
