using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories;

public class RoleRepository(GamestoreDbContext context) : Repository<Role>(context), IRoleRepository
{
    public async Task<Role?> GetByNameAsync(string name)
    {
        return await DbSet
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id)
    {
        return await DbSet
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Role>> GetAllWithPermissionsAsync()
    {
        return await DbSet
            .Include(r => r.Permissions)
            .ToListAsync();
    }
}
