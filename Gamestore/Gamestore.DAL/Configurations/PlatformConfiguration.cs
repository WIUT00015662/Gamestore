using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

/// <summary>
/// EF Core configuration for the Platform entity, including seed data.
/// </summary>
internal sealed class PlatformConfiguration : IEntityTypeConfiguration<Platform>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Platform> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Type).IsUnique();
        builder.Property(p => p.Type).IsRequired();

        SeedData(builder);
    }

    private static void SeedData(EntityTypeBuilder<Platform> builder)
    {
        var mobileId = new Guid("60DCCF95-D657-4EC3-B9D1-01529B4C40D3");
        var browserId = new Guid("40429DB0-1439-41AA-A420-73DCCBDFDC3D");
        var desktopId = new Guid("FD1E5957-93BC-4A81-A76B-B219B1B76D3F");
        var consoleId = new Guid("A76D2E28-AE0C-43C1-9E6E-5C75523DDC67");

        builder.HasData(
            new Platform { Id = mobileId, Type = "Mobile" },
            new Platform { Id = browserId, Type = "Browser" },
            new Platform { Id = desktopId, Type = "Desktop" },
            new Platform { Id = consoleId, Type = "Console" });
    }
}
