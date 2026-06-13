using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Typer.Domain.Entities;

namespace Typer.Infrastructure.Persistence.Configurations;

public class RankingLiveBaselineConfiguration : IEntityTypeConfiguration<RankingLiveBaseline>
{
    public void Configure(EntityTypeBuilder<RankingLiveBaseline> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => b.VipOnly).IsUnique();
        builder.Property(b => b.PointsJson).IsRequired();
    }
}
