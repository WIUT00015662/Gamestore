using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IPublisherRepository : IRepository<Publisher>
{
    Task<Publisher?> GetByCompanyNameAsync(string companyName);

    Task<Publisher?> GetByGameKeyAsync(string gameKey);
}
