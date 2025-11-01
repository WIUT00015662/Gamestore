using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class GameVendorOfferConfiguration : IEntityTypeConfiguration<GameVendorOffer>
{
    public void Configure(EntityTypeBuilder<GameVendorOffer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GameName).IsRequired();
        builder.Property(x => x.Vendor).IsRequired();
        builder.Property(x => x.PurchaseUrl).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.LastPolledPrice).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.GameId, x.Vendor, x.PurchaseUrl }).IsUnique();

        builder.HasOne(x => x.Game)
            .WithMany(g => g.VendorOffers)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
