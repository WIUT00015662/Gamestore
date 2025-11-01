using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class CommentBanConfiguration : IEntityTypeConfiguration<CommentBan>
{
    public void Configure(EntityTypeBuilder<CommentBan> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired();
        builder.Property(b => b.IsPermanent).IsRequired();

        builder.HasIndex(b => b.Name).IsUnique();
    }
}
