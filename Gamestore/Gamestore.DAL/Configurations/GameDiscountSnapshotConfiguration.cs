using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class GameDiscountSnapshotConfiguration : IEntityTypeConfiguration<GameDiscountSnapshot>
{
    public void Configure(EntityTypeBuilder<GameDiscountSnapshot> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GameName).IsRequired();
        builder.Property(x => x.Vendor).IsRequired();
        builder.Property(x => x.PurchaseUrl).IsRequired();
        builder.Property(x => x.OriginalPrice).HasPrecision(18, 2);
        builder.Property(x => x.DiscountedPrice).HasPrecision(18, 2);
        builder.Property(x => x.DiscountPercent).HasPrecision(5, 2);

        builder.HasIndex(x => x.PollingRunId);
        builder.HasIndex(x => x.PolledAt);

        builder.HasOne(x => x.Game)
            .WithMany()
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
