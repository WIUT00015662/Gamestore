namespace Gamestore.BLL.Services;

public class OrderSettings
{
    public Guid CustomerId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public int BankInvoiceValidityDays { get; set; } = 3;

    public int PaymentRetryCount { get; set; } = 3;
}
