using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;

namespace Gamestore.DAL.Repositories;

public class DiscountConfigurationRepository(GamestoreDbContext context) : Repository<DiscountConfiguration>(context), IDiscountConfigurationRepository
{
    public async Task<DiscountConfiguration?> GetActiveConfigurationAsync()
    {
        var result = await FindAsync(x => x.IsActive);
        return result.FirstOrDefault();
    }
}
