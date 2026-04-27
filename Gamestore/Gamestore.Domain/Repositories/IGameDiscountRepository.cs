using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IGameDiscountRepository : IRepository<GameDiscount>
{
    Task<IEnumerable<GameDiscount>> GetActiveDiscountsAsync();

    Task<IEnumerable<GameDiscount>> GetDiscountsByOfferIdAsync(Guid offerId);
}
