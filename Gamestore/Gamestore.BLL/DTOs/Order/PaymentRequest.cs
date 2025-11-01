namespace Gamestore.BLL.DTOs.Order;

public class PaymentRequest
{
    public required string Method { get; set; }

    public VisaPaymentModel? Model { get; set; }
}
