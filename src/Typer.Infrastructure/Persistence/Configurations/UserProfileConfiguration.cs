using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Typer.Domain.Entities;

namespace Typer.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.VipUser).HasDefaultValue(false);
        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.VipUser);

        builder.HasOne(p => p.SelectedTeam)
            .WithMany()
            .HasForeignKey(p => p.SelectedTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.SelectedPlayer)
            .WithMany()
            .HasForeignKey(p => p.SelectedPlayerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
