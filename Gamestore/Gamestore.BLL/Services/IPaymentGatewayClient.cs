using Gamestore.BLL.DTOs.Order;

namespace Gamestore.BLL.Services;

public interface IPaymentGatewayClient
{
    Task<IBoxPaymentResponse?> PayIBoxAsync(Guid userId, Guid orderId, double sum, DateTime paymentDate);

    Task<bool> PayVisaAsync(VisaPaymentModel model, double sum);
}
