using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class OrderGameConfiguration : IEntityTypeConfiguration<OrderGame>
{
    public void Configure(EntityTypeBuilder<OrderGame> builder)
    {
        builder.HasKey(og => new { og.OrderId, og.ProductId });

        builder.Property(og => og.Price).IsRequired();
        builder.Property(og => og.Quantity).IsRequired();

        builder.HasOne(og => og.Product)
            .WithMany()
            .HasForeignKey(og => og.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
