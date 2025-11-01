using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
{
    public void Configure(EntityTypeBuilder<Publisher> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.CompanyName).IsUnique();
        builder.Property(p => p.CompanyName).IsRequired();
    }
}
