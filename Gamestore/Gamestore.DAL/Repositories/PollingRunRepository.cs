using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;

namespace Gamestore.DAL.Repositories;

public class PollingRunRepository(GamestoreDbContext context) : Repository<PollingRun>(context), IPollingRunRepository
{
    public async Task<PollingRun?> GetLatestRunAsync()
    {
        var all = await GetAllAsync();
        return all.OrderByDescending(x => x.RunAt).FirstOrDefault();
    }
}
