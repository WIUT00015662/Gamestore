using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithDetailsAsync(Guid id);

    Task<Order?> GetOpenOrderByCustomerIdAsync(Guid customerId);

    Task<IEnumerable<Order>> GetByStatusesAsync(params OrderStatus[] statuses);
}