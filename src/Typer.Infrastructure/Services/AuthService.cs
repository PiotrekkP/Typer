using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Typer.Application.Auth.DTOs;
using Typer.Application.Auth.Interfaces;
using Typer.Application.Common.Models;
using Typer.Domain.Entities;
using Typer.Infrastructure.Identity;
using Typer.Infrastructure.Options;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _contextFactory = contextFactory;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result<AuthResponse>.Failure("Użytkownik o podanym adresie e-mail już istnieje.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var error = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result<AuthResponse>.Failure(error);
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        context.UserProfiles.Add(new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(CreateAuthResponse(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<AuthResponse>.Failure("Nieprawidłowy e-mail lub hasło.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Result<AuthResponse>.Failure("Nieprawidłowy e-mail lub hasło.");

        return Result<AuthResponse>.Success(CreateAuthResponse(user));
    }

    private AuthResponse CreateAuthResponse(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.DisplayName ?? user.Email ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse(tokenString, user.Id, user.Email ?? string.Empty, user.DisplayName ?? string.Empty);
    }
}
