using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IGenreRepository : IRepository<Genre>
{
    Task<Genre?> GetByNameAsync(string name);

    Task<IEnumerable<Genre>> GetByParentIdAsync(Guid parentId);

    Task<IEnumerable<Genre>> GetByGameKeyAsync(string gameKey);
}
