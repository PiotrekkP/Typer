using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Typer.Domain.Entities;

namespace Typer.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Code).HasMaxLength(3).IsRequired();
        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.LiveApiId);
    }
}
