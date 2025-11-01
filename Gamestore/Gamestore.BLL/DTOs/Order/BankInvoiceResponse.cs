namespace Gamestore.BLL.DTOs.Order;

public class BankInvoiceResponse
{
    public required byte[] Content { get; set; }

    public required string FileName { get; set; }
}
