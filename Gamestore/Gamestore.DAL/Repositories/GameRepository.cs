using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories;

/// <summary>
/// Game repository implementation.
/// </summary>
public class GameRepository(GamestoreDbContext context) : Repository<Game>(context), IGameRepository
{
    /// <inheritdoc/>
    public override async Task<Game?> GetByIdAsync(Guid id)
    {
        return await DbSet.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
    }

    /// <inheritdoc/>
    public override async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await DbSet.Where(g => !g.IsDeleted).ToListAsync();
    }

    /// <inheritdoc/>
    public override async Task<int> GetCountAsync()
    {
        return await DbSet.CountAsync(g => !g.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<Game?> GetByKeyAsync(string key)
    {
        return await DbSet.FirstOrDefaultAsync(g => g.Key == key && !g.IsDeleted);
    }

    public async Task<Game?> GetByKeyIncludingDeletedAsync(string key)
    {
        return await DbSet.FirstOrDefaultAsync(g => g.Key == key);
    }

    /// <inheritdoc/>
    public async Task<Game?> GetByKeyWithDetailsAsync(string key)
    {
        return await DbSet
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .FirstOrDefaultAsync(g => g.Key == key && !g.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<Game?> GetByIdWithDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
    }

    public async Task<Game?> GetByIdWithDetailsIncludingDeletedAsync(Guid id)
    {
        return await DbSet
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Game>> GetByGenreIdAsync(Guid genreId)
    {
        return await DbSet
            .Where(g => !g.IsDeleted && g.Genres.Any(genre => genre.Id == genreId))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Game>> GetByPlatformIdAsync(Guid platformId)
    {
        return await DbSet
            .Where(g => !g.IsDeleted && g.Platforms.Any(p => p.Id == platformId))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Game>> GetByPublisherNameAsync(string companyName)
    {
        return await DbSet
            .Include(g => g.Publisher)
            .Where(g => !g.IsDeleted && g.Publisher != null && g.Publisher.CompanyName == companyName)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Game>> GetAllWithDetailsAsync()
    {
        return await DbSet
            .Where(g => !g.IsDeleted)
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetAllWithDetailsIncludingDeletedAsync()
    {
        return await DbSet
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync()
    {
        return await DbSet.CountAsync(g => !g.IsDeleted);
    }

    /// <inheritdoc/>
    public IQueryable<Game> GetQueryable()
    {
        return DbSet
            .Where(g => !g.IsDeleted)
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .Include(g => g.Comments)
            .AsQueryable();
    }

    public IQueryable<Game> GetQueryableIncludingDeleted()
    {
        return DbSet
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .Include(g => g.Comments)
            .AsQueryable();
    }

    /// <inheritdoc/>
    public async Task IncrementViewCountAsync(Guid gameId)
    {
        var game = await DbSet.FirstOrDefaultAsync(g => g.Id == gameId);
        if (game != null)
        {
            game.ViewCount++;
            DbSet.Update(game);
        }
    }
}
