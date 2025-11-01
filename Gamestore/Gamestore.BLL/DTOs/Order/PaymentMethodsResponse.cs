namespace Gamestore.BLL.DTOs.Order;

public class PaymentMethodsResponse
{
    public IEnumerable<PaymentMethodResponse> PaymentMethods { get; set; } = [];
}
