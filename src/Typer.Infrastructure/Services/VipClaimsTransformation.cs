using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

/// <summary>
/// Dodaje rolę Vip do cookie na podstawie UserProfile.VipUser — bez wymuszania ponownego logowania.
/// </summary>
public class VipClaimsTransformation : IClaimsTransformation
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public VipClaimsTransformation(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        if (principal.IsInRole(VipRoleSeeder.VipRoleName))
            return principal;

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return principal;

        await using var context = await _contextFactory.CreateDbContextAsync();
        var isVip = await context.UserProfiles
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.VipUser);

        if (!isVip)
            return principal;

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.Role, VipRoleSeeder.VipRoleName));
        principal.AddIdentity(identity);
        return principal;
    }
}
