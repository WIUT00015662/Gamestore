using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;

namespace Gamestore.DAL.Repositories;

public class GameDiscountRepository(GamestoreDbContext context) : Repository<GameDiscount>(context), IGameDiscountRepository
{
    public async Task<IEnumerable<GameDiscount>> GetActiveDiscountsAsync()
    {
        return await FindAsync(x => x.IsCurrentlyActive);
    }

    public async Task<IEnumerable<GameDiscount>> GetDiscountsByOfferIdAsync(Guid offerId)
    {
        return await FindAsync(x => x.GameVendorOfferId == offerId);
    }
}
