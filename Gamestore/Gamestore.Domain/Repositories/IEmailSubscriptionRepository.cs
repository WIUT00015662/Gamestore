using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IEmailSubscriptionRepository : IRepository<EmailSubscription>
{
    Task<EmailSubscription?> GetByEmailAsync(string email);
}
