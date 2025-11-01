namespace Gamestore.BLL.DTOs.Order;

public class IBoxPaymentResponse
{
    public Guid UserId { get; set; }

    public Guid OrderId { get; set; }

    public DateTime PaymentDate { get; set; }

    public double Sum { get; set; }
}
