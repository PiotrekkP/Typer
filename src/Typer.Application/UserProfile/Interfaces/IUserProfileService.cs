using Typer.Application.UserProfile.DTOs;

namespace Typer.Application.UserProfile.Interfaces;

public interface IUserProfileService
{
    Task<UserSelectionDto?> GetSelectionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Zwraca publiczny profil gracza: dane profilowe, pozycja w rankingu i historia typów.
    /// Zwraca null jeśli użytkownik nie istnieje.
    /// </summary>
    Task<UserPublicProfileDto?> GetPublicProfileAsync(string userId, CancellationToken cancellationToken = default);
}
