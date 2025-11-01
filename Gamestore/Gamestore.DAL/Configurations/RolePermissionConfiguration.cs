using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionEntry>
{
    public void Configure(EntityTypeBuilder<RolePermissionEntry> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(x => new { x.RoleId, x.Permission });
        builder.Property(x => x.Permission).IsRequired();
    }
}
