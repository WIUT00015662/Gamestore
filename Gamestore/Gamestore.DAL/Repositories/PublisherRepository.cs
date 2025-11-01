using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories;

public class PublisherRepository(GamestoreDbContext context) : Repository<Publisher>(context), IPublisherRepository
{
    public async Task<Publisher?> GetByCompanyNameAsync(string companyName)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.CompanyName == companyName);
    }

    public async Task<Publisher?> GetByGameKeyAsync(string gameKey)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Games.Any(g => g.Key == gameKey));
    }
}
