using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IGameRepository : IRepository<Game>
{
    Task<Game?> GetByKeyAsync(string key);

    Task<Game?> GetByKeyIncludingDeletedAsync(string key);

    Task<Game?> GetByKeyWithDetailsAsync(string key);

    Task<Game?> GetByIdWithDetailsAsync(Guid id);

    Task<Game?> GetByIdWithDetailsIncludingDeletedAsync(Guid id);

    Task<IEnumerable<Game>> GetByGenreIdAsync(Guid genreId);

    Task<IEnumerable<Game>> GetByPlatformIdAsync(Guid platformId);

    Task<IEnumerable<Game>> GetByPublisherNameAsync(string companyName);

    Task<IEnumerable<Game>> GetAllWithDetailsAsync();

    Task<IEnumerable<Game>> GetAllWithDetailsIncludingDeletedAsync();

    Task<int> GetTotalCountAsync();

    IQueryable<Game> GetQueryable();

    IQueryable<Game> GetQueryableIncludingDeleted();

    Task IncrementViewCountAsync(Guid gameId);
}
