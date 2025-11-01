using System.Linq.Expressions;
using Gamestore.DAL.Data;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories;

/// <summary>
/// Repository implementation.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public class Repository<T>(GamestoreDbContext context) : IRepository<T>
    where T : class
{
    /// <summary>
    /// Gets the context.
    /// </summary>
    protected GamestoreDbContext Context { get; } = context;

    /// <summary>
    /// Gets the DbSet.
    /// </summary>
    protected DbSet<T> DbSet { get; } = context.Set<T>();

    /// <inheritdoc/>
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<int> GetCountAsync()
    {
        return await DbSet.CountAsync();
    }

    /// <inheritdoc/>
    public virtual async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    /// <inheritdoc/>
    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    /// <inheritdoc/>
    public virtual void Delete(T entity)
    {
        DbSet.Remove(entity);
    }
}
