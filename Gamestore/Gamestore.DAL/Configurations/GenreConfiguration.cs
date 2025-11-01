using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

/// <summary>
/// EF Core configuration for the Genre entity, including hierarchy and seed data.
/// </summary>
internal sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.HasKey(g => g.Id);
        builder.HasIndex(g => g.Name).IsUnique();
        builder.Property(g => g.Name).IsRequired();

        builder.HasOne(g => g.ParentGenre)
            .WithMany(g => g.SubGenres)
            .HasForeignKey(g => g.ParentGenreId)
            .OnDelete(DeleteBehavior.Restrict);

        SeedData(builder);
    }

    private static void SeedData(EntityTypeBuilder<Genre> builder)
    {
        var strategyId = new Guid("3FDFDA42-5F9F-4F50-90A3-B0F9F4C83297");
        var rtsId = new Guid("FCDBD9B9-32E0-4190-964F-BE636CC1CEB8");
        var tbsId = new Guid("2E98834A-1196-41E3-93B9-5DCF79C3504F");
        var rpgId = new Guid("E219B857-AF30-4F06-97A7-23FCA0B061F5");
        var sportsId = new Guid("10C8ABD7-0901-4D25-96CD-460ED43B002B");
        var racesId = new Guid("6CC93CE8-8B02-489C-A2C0-5C45E12380CF");
        var rallyId = new Guid("534F0BBC-8E8A-465A-9185-4EEAD9BB8881");
        var arcadeId = new Guid("826E499C-ABE9-4F43-A7CC-6E6361B8EAF3");
        var formulaId = new Guid("08044742-F1B2-4E1E-BE70-2C03077E3570");
        var offRoadId = new Guid("DA7216D9-61E6-4B5F-989D-FAEE4A0DF753");
        var actionId = new Guid("58413A2F-534A-488E-8066-8F1E48611B35");
        var fpsId = new Guid("52514583-2213-4DEE-AABE-FEBFD2F95846");
        var tpsId = new Guid("3AEA7CBC-EDBA-40D0-9709-5655F8476EF6");
        var adventureId = new Guid("FBA94F42-23EF-4E20-9871-73C51852517A");
        var puzzleId = new Guid("F426CE2D-3F57-430E-9614-66E0313BA5A1");

        builder.HasData(
            new Genre { Id = strategyId, Name = "Strategy", ParentGenreId = null },
            new Genre { Id = rtsId, Name = "RTS", ParentGenreId = strategyId },
            new Genre { Id = tbsId, Name = "TBS", ParentGenreId = strategyId },
            new Genre { Id = rpgId, Name = "RPG", ParentGenreId = null },
            new Genre { Id = sportsId, Name = "Sports", ParentGenreId = null },
            new Genre { Id = racesId, Name = "Races", ParentGenreId = null },
            new Genre { Id = rallyId, Name = "Rally", ParentGenreId = racesId },
            new Genre { Id = arcadeId, Name = "Arcade", ParentGenreId = racesId },
            new Genre { Id = formulaId, Name = "Formula", ParentGenreId = racesId },
            new Genre { Id = offRoadId, Name = "Off-road", ParentGenreId = racesId },
            new Genre { Id = actionId, Name = "Action", ParentGenreId = null },
            new Genre { Id = fpsId, Name = "FPS", ParentGenreId = actionId },
            new Genre { Id = tpsId, Name = "TPS", ParentGenreId = actionId },
            new Genre { Id = adventureId, Name = "Adventure", ParentGenreId = null },
            new Genre { Id = puzzleId, Name = "Puzzle & Skill", ParentGenreId = null });
    }
}
