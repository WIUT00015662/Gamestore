using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class DiscountConfigurationEntityConfiguration : IEntityTypeConfiguration<DiscountConfiguration>
{
    public void Configure(EntityTypeBuilder<DiscountConfiguration> builder)
    {
        builder.HasKey(dc => dc.Id);
        builder.Property(dc => dc.DiscountProbability).IsRequired().HasPrecision(5, 4);
        builder.Property(dc => dc.DiscountPercentageMin).IsRequired().HasPrecision(5, 2);
        builder.Property(dc => dc.DiscountPercentageMax).IsRequired().HasPrecision(5, 2);
        builder.Property(dc => dc.TimeWindowMinutes).IsRequired();
        builder.Property(dc => dc.DiscountRevertProbability).IsRequired().HasPrecision(5, 4);
        builder.Property(dc => dc.IsActive).IsRequired();
    }
}
