using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IPollingRunRepository : IRepository<PollingRun>
{
    Task<PollingRun?> GetLatestRunAsync();
}
