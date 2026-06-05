using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Typer.Domain.Entities;

namespace Typer.Infrastructure.Persistence.Configurations;

public class GoalScorerConfiguration : IEntityTypeConfiguration<GoalScorer>
{
    public void Configure(EntityTypeBuilder<GoalScorer> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.PlayerName).HasMaxLength(150).IsRequired();

        builder.HasOne(g => g.Match)
            .WithMany(m => m.GoalScorers)
            .HasForeignKey(g => g.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.Player)
            .WithMany()
            .HasForeignKey(g => g.PlayerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
