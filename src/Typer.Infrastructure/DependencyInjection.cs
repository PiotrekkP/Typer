using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Typer.Application.Auth.Interfaces;
using Typer.Application.Common.Interfaces;
using Typer.Application.Integrations.ApiFootball;
using Typer.Application.Matches.Interfaces;
using Typer.Application.Players.Interfaces;
using Typer.Application.Predictions.Interfaces;
using Typer.Application.Rankings.Interfaces;
using Typer.Application.Scoring.Interfaces;
using Typer.Application.Teams.Interfaces;
using Typer.Application.UserProfile.Interfaces;
using Typer.Infrastructure.Identity;
using Typer.Infrastructure.Integrations.ApiFootball;
using Typer.Infrastructure.Options;
using Typer.Infrastructure.Persistence;
using Typer.Infrastructure.Services;

namespace Typer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<ApiFootballOptions>(configuration.GetSection(ApiFootballOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

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
        services.AddScoped<IMatchLifecycleService, MatchLifecycleService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IPredictionService, PredictionService>();
        services.AddScoped<IRankingService, RankingService>();
        services.AddScoped<IScoringService, ScoringService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<ILiveResultsSyncService, LiveResultsSyncService>();

        services.AddHttpClient<IApiFootballClient, ApiFootballClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiFootballOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            if (!string.IsNullOrWhiteSpace(options.ApiKey))
                client.DefaultRequestHeaders.TryAddWithoutValidation("x-apisports-key", options.ApiKey);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    public static IServiceCollection AddMatchStatusBackgroundService(this IServiceCollection services)
    {
        services.AddHostedService<Background.MatchStatusBackgroundService>();
        services.AddHostedService<Background.LiveScoringBackgroundService>();
        services.AddHostedService<Background.LiveResultsBackgroundService>();
        return services;
    }
}
