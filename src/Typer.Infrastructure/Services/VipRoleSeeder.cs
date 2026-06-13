using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typer.Infrastructure.Identity;
using Typer.Infrastructure.Options;
using Typer.Infrastructure.Persistence;

namespace Typer.Infrastructure.Services;

public static class VipRoleSeeder
{
    public const string VipRoleName = "Vip";

    public static async Task EnsureAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<VipOptions>>().Value;
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("VipRoleSeeder");

        if (!await roleManager.RoleExistsAsync(VipRoleName))
        {
            var createRole = await roleManager.CreateAsync(new IdentityRole(VipRoleName));
            if (!createRole.Succeeded)
            {
                logger.LogWarning("Nie udało się utworzyć roli Vip.");
                return;
            }
        }

        var emails = options.VipEmails
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        foreach (var email in emails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                logger.LogInformation("Użytkownik {Email} nie istnieje — VIP zostanie nadany po rejestracji.", email);
                continue;
            }

            var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile is null)
                continue;

            if (!profile.VipUser)
            {
                profile.VipUser = true;
                profile.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                logger.LogInformation("Ustawiono VipUser=true dla {Email}.", email);
            }
        }

        var vipUserIds = await context.UserProfiles
            .AsNoTracking()
            .Where(p => p.VipUser)
            .Select(p => p.UserId)
            .ToListAsync();

        var vipUserIdSet = vipUserIds.ToHashSet(StringComparer.Ordinal);

        foreach (var userId in vipUserIds)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                continue;

            if (!await userManager.IsInRoleAsync(user, VipRoleName))
            {
                var result = await userManager.AddToRoleAsync(user, VipRoleName);
                if (result.Succeeded)
                    logger.LogInformation("Nadano rolę Vip użytkownikowi {UserId}.", userId);
            }
        }

        var usersInVipRole = await userManager.GetUsersInRoleAsync(VipRoleName);
        foreach (var user in usersInVipRole)
        {
            if (vipUserIdSet.Contains(user.Id))
                continue;

            var result = await userManager.RemoveFromRoleAsync(user, VipRoleName);
            if (result.Succeeded)
                logger.LogInformation("Usunięto rolę Vip użytkownikowi {Email} (VipUser=false).", user.Email);
        }
    }
}
