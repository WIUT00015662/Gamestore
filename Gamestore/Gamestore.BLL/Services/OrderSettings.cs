namespace Gamestore.BLL.Services;

public class OrderSettings
{
    public int BankInvoiceValidityDays { get; set; } = 3;

    public int PaymentRetryCount { get; set; } = 3;
}
