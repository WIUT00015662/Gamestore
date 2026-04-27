using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;

namespace Gamestore.DAL.Repositories;

/// <summary>
/// Email subscription repository implementation.
/// </summary>
public class EmailSubscriptionRepository(GamestoreDbContext context) : Repository<EmailSubscription>(context), IEmailSubscriptionRepository
{
    /// <inheritdoc/>
    public async Task<EmailSubscription?> GetByEmailAsync(string email)
    {
        var result = await FindAsync(x => x.Email == email);
        return result.FirstOrDefault();
    }
}
