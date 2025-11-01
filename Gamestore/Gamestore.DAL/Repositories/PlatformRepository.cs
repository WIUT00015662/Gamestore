using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories;

/// <summary>
/// Platform repository implementation.
/// </summary>
public class PlatformRepository(GamestoreDbContext context) : Repository<Platform>(context), IPlatformRepository
{
    /// <inheritdoc/>
    public async Task<Platform?> GetByTypeAsync(string type)
    {
        return await DbSet.FirstOrDefaultAsync(p => p.Type == type);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Platform>> GetByGameKeyAsync(string gameKey)
    {
        return await Context.Games
            .Where(g => g.Key == gameKey)
            .SelectMany(g => g.Platforms)
            .ToListAsync();
    }
}
