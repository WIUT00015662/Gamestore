using System.Text.Json.Serialization;

namespace Gamestore.BLL.DTOs.Comment;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BanDurationType
{
    OneHour,
    OneDay,
    OneWeek,
    OneMonth,
    Permanent,
}
