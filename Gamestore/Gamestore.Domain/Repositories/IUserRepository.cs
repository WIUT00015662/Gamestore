using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByNameAsync(string name);

    Task<User?> GetByIdWithRolesAsync(Guid id);

    Task<IEnumerable<User>> GetAllWithRolesAsync();
}
