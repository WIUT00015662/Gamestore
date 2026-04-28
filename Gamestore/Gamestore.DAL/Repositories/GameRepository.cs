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
        return await DbSet.FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <inheritdoc/>
    public override async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    /// <inheritdoc/>
    public override async Task<int> GetCountAsync()
    {
        return await DbSet.CountAsync();
    }

    /// <inheritdoc/>
    public async Task<Game?> GetByKeyAsync(string key)
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
            .Include(g => g.VendorOffers)
            .FirstOrDefaultAsync(g => g.Key == key);
    }

    /// <inheritdoc/>
    public async Task<Game?> GetByIdWithDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .Include(g => g.VendorOffers)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Game>> GetByGenreIdAsync(Guid genreId)
    {
        return await DbSet
            .Where(g => g.Genres.Any(genre => genre.Id == genreId))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Game>> GetByPlatformIdAsync(Guid platformId)
    {
        return await DbSet
            .Where(g => g.Platforms.Any(p => p.Id == platformId))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Game>> GetByPublisherNameAsync(string companyName)
    {
        return await DbSet
            .Include(g => g.Publisher)
            .Where(g => g.Publisher != null && g.Publisher.CompanyName == companyName)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Game>> GetAllWithDetailsAsync()
    {
        return await DbSet
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .Include(g => g.VendorOffers)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync()
    {
        return await DbSet.CountAsync();
    }

    /// <inheritdoc/>
    public IQueryable<Game> GetQueryable()
    {
        return DbSet
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Publisher)
            .Include(g => g.Comments)
            .Include(g => g.VendorOffers)
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
