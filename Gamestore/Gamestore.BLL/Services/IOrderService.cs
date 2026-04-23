using Gamestore.BLL.DTOs.Order;

namespace Gamestore.BLL.Services;

public interface IOrderService
{
    Task AddGameToCartAsync(string gameKey, Guid userId);

    Task RemoveGameFromCartAsync(string gameKey, Guid userId);

    Task AddGameToOrderAsync(Guid orderId, string gameKey);

    Task UpdateOrderDetailQuantityAsync(Guid detailId, int count);

    Task DeleteOrderDetailAsync(Guid detailId);

    Task ShipOrderAsync(Guid orderId);

    /// <summary>
    /// Get current user's orders (history).
    /// </summary>
    Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(Guid userId);

    /// <summary>
    /// Get specific order by current user (with ownership check).
    /// </summary>
    Task<OrderResponse> GetMyOrderByIdAsync(Guid orderId, Guid userId);

    /// <summary>
    /// Get order details for current user's order.
    /// </summary>
    Task<IEnumerable<OrderGameResponse>> GetMyOrderDetailsAsync(Guid orderId, Guid userId);

    /// <summary>
    /// Get all orders (admin/manager access only).
    /// </summary>
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();

    /// <summary>
    /// Get any order by ID (admin/manager access only).
    /// </summary>
    Task<OrderResponse> GetOrderByIdAsync(Guid orderId);

    /// <summary>
    /// Get order details for any order (admin/manager access only).
    /// </summary>
    Task<IEnumerable<OrderGameResponse>> GetOrderDetailsAsync(Guid orderId);

    Task<IEnumerable<OrderGameResponse>> GetCartAsync(Guid userId);

    PaymentMethodsResponse GetPaymentMethods();

    Task<BankInvoiceResponse> PayByBankAsync(Guid userId);

    Task PayByVisaAsync(VisaPaymentModel model, Guid userId);
}
