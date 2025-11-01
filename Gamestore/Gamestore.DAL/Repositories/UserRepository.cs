using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories;

public class UserRepository(GamestoreDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByNameAsync(string name)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Name == name);
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid id)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> GetAllWithRolesAsync()
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }
}
