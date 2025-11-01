using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;

namespace Gamestore.DAL.Repositories;

/// <summary>
/// Unit of work implementation.
/// </summary>
public class UnitOfWork(GamestoreDbContext context) : IUnitOfWork
{
    private readonly GamestoreDbContext _context = context;
    private bool _disposed;
    private IGameRepository? _games;
    private IGenreRepository? _genres;
    private IPlatformRepository? _platforms;
    private IPublisherRepository? _publishers;
    private IOrderRepository? _orders;
    private ICommentRepository? _comments;
    private ICommentBanRepository? _commentBans;
    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IRepository<GameVendorOffer>? _gameVendorOffers;
    private IRepository<GameDiscountSnapshot>? _gameDiscountSnapshots;

    /// <inheritdoc/>
    public IGameRepository Games => _games ??= new GameRepository(_context);

    /// <inheritdoc/>
    public IGenreRepository Genres => _genres ??= new GenreRepository(_context);

    /// <inheritdoc/>
    public IPlatformRepository Platforms => _platforms ??= new PlatformRepository(_context);

    /// <inheritdoc/>
    public IPublisherRepository Publishers => _publishers ??= new PublisherRepository(_context);

    /// <inheritdoc/>
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

    /// <inheritdoc/>
    public ICommentRepository Comments => _comments ??= new CommentRepository(_context);

    /// <inheritdoc/>
    public ICommentBanRepository CommentBans => _commentBans ??= new CommentBanRepository(_context);

    public IUserRepository Users => _users ??= new UserRepository(_context);

    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);

    public IRepository<GameVendorOffer> GameVendorOffers => _gameVendorOffers ??= new Repository<GameVendorOffer>(_context);

    public IRepository<GameDiscountSnapshot> GameDiscountSnapshots => _gameDiscountSnapshots ??= new Repository<GameDiscountSnapshot>(_context);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }
    }
}
