using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typer.Infrastructure.Identity;
using Typer.Infrastructure.Options;

namespace Typer.Infrastructure.Services;

public static class AdminRoleSeeder
{
    public const string AdminRoleName = "Admin";

    public static async Task EnsureAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<AdminOptions>>().Value;
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminRoleSeeder");

        if (!await roleManager.RoleExistsAsync(AdminRoleName))
        {
            var createRole = await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
            if (!createRole.Succeeded)
            {
                logger.LogWarning("Nie udało się utworzyć roli Admin.");
                return;
            }
        }

        var emails = options.AdminEmails
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (!emails.Any())
        {
            logger.LogInformation("Brak Admin:AdminEmails w konfiguracji — pominięto przypisanie roli.");
            return;
        }

        foreach (var email in emails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                logger.LogInformation("Użytkownik {Email} nie istnieje — rola Admin zostanie nadana po rejestracji.", email);
                continue;
            }

            if (!await userManager.IsInRoleAsync(user, AdminRoleName))
            {
                var result = await userManager.AddToRoleAsync(user, AdminRoleName);
                if (result.Succeeded)
                    logger.LogInformation("Nadano rolę Admin użytkownikowi {Email}.", email);
                else
                    logger.LogWarning("Nie udało się nadać roli Admin użytkownikowi {Email}.", email);
            }
        }
    }
}
