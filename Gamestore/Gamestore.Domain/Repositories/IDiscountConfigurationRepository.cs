using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IDiscountConfigurationRepository : IRepository<DiscountConfiguration>
{
    Task<DiscountConfiguration?> GetActiveConfigurationAsync();
}
