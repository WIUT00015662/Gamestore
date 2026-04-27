using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class PollingRunConfiguration : IEntityTypeConfiguration<PollingRun>
{
    public void Configure(EntityTypeBuilder<PollingRun> builder)
    {
        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.RunAt).IsRequired();
        builder.Property(pr => pr.Status).IsRequired();
        builder.Property(pr => pr.ProcessedOffersCount).IsRequired();
        builder.Property(pr => pr.NewDiscountsCreated).IsRequired();
        builder.Property(pr => pr.DiscountsReverted).IsRequired();

        builder.HasIndex(pr => pr.RunAt).IsDescending();
    }
}
