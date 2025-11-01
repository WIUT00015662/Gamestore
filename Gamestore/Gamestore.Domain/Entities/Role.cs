namespace Gamestore.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public bool IsSystem { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];

    public ICollection<RolePermissionEntry> Permissions { get; set; } = [];
}
