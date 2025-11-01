namespace Gamestore.Domain.Entities;

public enum OrderStatus
{
    Open = 0,
    Checkout = 1,
    Paid = 2,
    Cancelled = 3,
    Shipped = 4,
}
