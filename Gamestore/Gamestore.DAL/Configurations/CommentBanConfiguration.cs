using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class CommentBanConfiguration : IEntityTypeConfiguration<CommentBan>
{
    public void Configure(EntityTypeBuilder<CommentBan> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.Name).IsRequired();
        builder.Property(b => b.IsPermanent).IsRequired();

        builder.HasOne(b => b.User)
            .WithMany(u => u.CommentBans)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.UserId).IsUnique();
    }
}
