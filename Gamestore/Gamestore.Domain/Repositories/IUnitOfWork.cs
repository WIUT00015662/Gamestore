using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGameRepository Games { get; }

    IGenreRepository Genres { get; }

    IPlatformRepository Platforms { get; }

    IPublisherRepository Publishers { get; }

    IOrderRepository Orders { get; }

    ICommentRepository Comments { get; }

    ICommentBanRepository CommentBans { get; }

    IUserRepository Users { get; }

    IRoleRepository Roles { get; }

    IRepository<GameVendorOffer> GameVendorOffers { get; }

    IRepository<GameDiscountSnapshot> GameDiscountSnapshots { get; }

    Task<int> SaveChangesAsync();
}
