using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IPlatformRepository : IRepository<Platform>
{
    Task<Platform?> GetByTypeAsync(string type);

    Task<IEnumerable<Platform>> GetByGameKeyAsync(string gameKey);
}
