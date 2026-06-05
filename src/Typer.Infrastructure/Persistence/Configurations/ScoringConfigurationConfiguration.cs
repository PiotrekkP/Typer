using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Typer.Domain.Entities;

namespace Typer.Infrastructure.Persistence.Configurations;

public class ScoringConfigurationConfiguration : IEntityTypeConfiguration<ScoringConfiguration>
{
    public void Configure(EntityTypeBuilder<ScoringConfiguration> builder)
    {
        builder.ToTable("ScoringConfigurations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasData(new ScoringConfiguration
        {
            Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            Name = "Domyślna",
            IsActive = true,
            CorrectWinnerPoints = 2,
            CorrectGoalDifferenceBonus = 1,
            ExactScorePoints = 5,
            FavoriteTeamMultiplier = 2.0,
            TournamentWinnerBonus = 20,
            FavoritePlayerGoalBonus = 3,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
