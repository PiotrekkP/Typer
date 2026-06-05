using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Typer.Application.Common.Interfaces;
using Typer.Domain.Entities;
using Typer.Infrastructure.Identity;

namespace Typer.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<GoalScorer> GoalScorers => Set<GoalScorer>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ScoringConfiguration> ScoringConfigurations => Set<ScoringConfiguration>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
