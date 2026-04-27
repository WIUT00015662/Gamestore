using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class GameDiscountConfiguration : IEntityTypeConfiguration<GameDiscount>
{
    public void Configure(EntityTypeBuilder<GameDiscount> builder)
    {
        builder.HasKey(gd => gd.Id);
        builder.Property(gd => gd.DiscountPercent).IsRequired().HasPrecision(5, 2);
        builder.Property(gd => gd.IsCurrentlyActive).IsRequired();
        builder.Property(gd => gd.CreatedAt).IsRequired();

        builder.HasOne(gd => gd.GameVendorOffer)
            .WithMany(gvo => gvo.Discounts)
            .HasForeignKey(gd => gd.GameVendorOfferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
