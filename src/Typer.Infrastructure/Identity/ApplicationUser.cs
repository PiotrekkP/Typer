using Microsoft.AspNetCore.Identity;

namespace Typer.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
