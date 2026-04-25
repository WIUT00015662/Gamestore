using System.Text.Json.Serialization;

namespace Gamestore.BLL.DTOs.Order;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethodType
{
    Bank,
    Visa,
}
