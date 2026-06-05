namespace Typer.Application.Auth.Interfaces;

using Typer.Application.Auth.DTOs;
using Typer.Application.Common.Models;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
