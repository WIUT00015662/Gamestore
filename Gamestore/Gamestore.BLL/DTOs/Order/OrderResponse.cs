namespace Gamestore.BLL.DTOs.Order;

public class OrderResponse
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public DateTime? Date { get; set; }
}
