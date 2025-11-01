using Gamestore.BLL.DTOs.Order;

namespace Gamestore.BLL.Services;

public interface IOrderService
{
    Task AddGameToCartAsync(string gameKey);

    Task RemoveGameFromCartAsync(string gameKey);

    Task AddGameToOrderAsync(Guid orderId, string gameKey);

    Task UpdateOrderDetailQuantityAsync(Guid detailId, int count);

    Task DeleteOrderDetailAsync(Guid detailId);

    Task ShipOrderAsync(Guid orderId);

    Task<IEnumerable<OrderResponse>> GetOrdersAsync();

    Task<OrderResponse> GetOrderByIdAsync(Guid orderId);

    Task<IEnumerable<OrderGameResponse>> GetOrderDetailsAsync(Guid orderId);

    Task<IEnumerable<OrderGameResponse>> GetCartAsync();

    PaymentMethodsResponse GetPaymentMethods();

    Task<BankInvoiceResponse> PayByBankAsync();

    Task<IBoxPaymentResponse> PayByIBoxAsync();

    Task PayByVisaAsync(VisaPaymentModel model);
}
