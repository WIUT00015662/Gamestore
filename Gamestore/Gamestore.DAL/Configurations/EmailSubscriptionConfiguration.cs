using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class EmailSubscriptionConfiguration : IEntityTypeConfiguration<EmailSubscription>
{
    public void Configure(EntityTypeBuilder<EmailSubscription> builder)
    {
        builder.HasKey(es => es.Id);
        builder.Property(es => es.Email).IsRequired();
        builder.Property(es => es.IsActive).IsRequired();
        builder.Property(es => es.SubscribedAt).IsRequired();
        builder.HasIndex(es => es.Email).IsUnique();
    }
}
