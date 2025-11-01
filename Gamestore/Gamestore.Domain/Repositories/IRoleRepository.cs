using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);

    Task<Role?> GetByIdWithPermissionsAsync(Guid id);

    Task<IEnumerable<Role>> GetAllWithPermissionsAsync();
}
