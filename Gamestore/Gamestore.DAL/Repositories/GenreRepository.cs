using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories;

/// <summary>
/// Genre repository implementation.
/// </summary>
public class GenreRepository(GamestoreDbContext context) : Repository<Genre>(context), IGenreRepository
{
    /// <inheritdoc/>
    public async Task<Genre?> GetByNameAsync(string name)
    {
        return await DbSet.FirstOrDefaultAsync(g => g.Name == name);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Genre>> GetByParentIdAsync(Guid parentId)
    {
        return await DbSet
            .Where(g => g.ParentGenreId == parentId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Genre>> GetByGameKeyAsync(string gameKey)
    {
        return await Context.Games
            .Where(g => g.Key == gameKey)
            .SelectMany(g => g.Genres)
            .ToListAsync();
    }
}
