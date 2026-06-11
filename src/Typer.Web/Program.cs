using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Typer.Application;
using Typer.Domain.Entities;
using Typer.Infrastructure;
using Typer.Infrastructure.Identity;
using Typer.Infrastructure.Persistence;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Web.Components;
using Typer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMatchStatusBackgroundService();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IUserSelectionEvents, UserSelectionEvents>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// ── Endpointy auth (obsługa cookie) ──────────────────────────────
app.MapPost("/account/login", async (
    [FromForm] string email,
    [FromForm] string password,
    SignInManager<ApplicationUser> signInManager) =>
{
    var result = await signInManager.PasswordSignInAsync(
        email, password, isPersistent: true, lockoutOnFailure: false);

    return result.Succeeded
        ? Results.Redirect("/")
        : Results.Redirect("/logowanie?error=invalid");
}).DisableAntiforgery();

app.MapPost("/account/register", async (
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string displayName,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context) =>
{
    var user = new ApplicationUser
    {
        UserName = email,
        Email    = email,
        DisplayName = displayName
    };

    var result = await userManager.CreateAsync(user, password);

    if (!result.Succeeded)
    {
        var code = result.Errors.First().Code;
        return Results.Redirect($"/rejestracja?error={code}");
    }

    context.UserProfiles.Add(new UserProfile
    {
        Id          = Guid.NewGuid(),
        UserId      = user.Id,
        DisplayName = displayName,
        CreatedAt   = DateTime.UtcNow
    });
    await context.SaveChangesAsync();
    await signInManager.SignInAsync(user, isPersistent: true);

    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/logowanie");
}).DisableAntiforgery();
// ─────────────────────────────────────────────────────────────────

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var lifecycle = scope.ServiceProvider.GetRequiredService<IMatchLifecycleService>();
    await lifecycle.AdvanceStatusesAsync();

    var scoring = scope.ServiceProvider.GetRequiredService<IScoringService>();
    await scoring.UpdateLiveScoresForInProgressMatchesAsync();
}

app.Run();
