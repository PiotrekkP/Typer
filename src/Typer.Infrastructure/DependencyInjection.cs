using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Typer.Application.Auth.Interfaces;
using Typer.Application.Common.Interfaces;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Players.Interfaces;
using Typer.Application.Predictions.Interfaces;
using Typer.Application.Rankings.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Application.Teams.Interfaces;
using Typer.Application.UserProfile.Interfaces;
using Typer.Infrastructure.Identity;
using Typer.Infrastructure.Options;
using Typer.Infrastructure.Persistence;
using Typer.Infrastructure.Services;

namespace Typer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IPredictionService, PredictionService>();
        services.AddScoped<IRankingService, RankingService>();
        services.AddScoped<IScoringService, ScoringService>();
        services.AddScoped<IUserProfileService, UserProfileService>();

        return services;
    }
}
