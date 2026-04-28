using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

/// <summary>
/// EF Core configuration for the Game entity and its many-to-many relationships.
/// </summary>
internal sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(g => g.Id);
        builder.HasIndex(g => g.Key).IsUnique();
        builder.Property(g => g.Name).IsRequired();
        builder.Property(g => g.Key).IsRequired();
        builder.Property(g => g.UnitInStock).IsRequired();
        builder.Property(g => g.PublishDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        builder.Property(g => g.ViewCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(g => g.Publisher)
            .WithMany(p => p.Games)
            .HasForeignKey(g => g.PublisherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Genres)
            .WithMany()
            .UsingEntity<GameGenre>(
                r => r.HasOne<Genre>().WithMany().HasForeignKey(gg => gg.GenreId).OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Game>().WithMany().HasForeignKey(gg => gg.GameId).OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("GameGenres");
                    j.HasKey(gg => new { gg.GameId, gg.GenreId });
                });

        builder.HasMany(g => g.Platforms)
            .WithMany()
            .UsingEntity<GamePlatform>(
                r => r.HasOne<Platform>().WithMany().HasForeignKey(gp => gp.PlatformId).OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Game>().WithMany().HasForeignKey(gp => gp.GameId).OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("GamePlatforms");
                    j.HasKey(gp => new { gp.GameId, gp.PlatformId });
                });
    }
}
