using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.Status).IsRequired();

        builder.HasMany(o => o.OrderGames)
            .WithOne(og => og.Order)
            .HasForeignKey(og => og.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
