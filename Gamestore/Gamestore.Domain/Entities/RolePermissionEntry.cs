namespace Gamestore.Domain.Entities;

public class RolePermissionEntry
{
    public Guid RoleId { get; set; }

    public required string Permission { get; set; }

    public Role? Role { get; set; }
}
