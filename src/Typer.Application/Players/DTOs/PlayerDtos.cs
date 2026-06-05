namespace Typer.Application.Players.DTOs;

public record PlayerDto(
    Guid Id,
    Guid TeamId,
    string FirstName,
    string LastName,
    int JerseyNumber,
    string? Position,
    string? Club,
    bool IsMvp,
    string? PhotoUrl,
    string? TeamName,
    string? TeamFlagUrl);

public record SelectPlayerRequest(Guid PlayerId);
