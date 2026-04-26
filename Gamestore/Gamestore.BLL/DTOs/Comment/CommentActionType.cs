using System.Text.Json.Serialization;

namespace Gamestore.BLL.DTOs.Comment;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CommentActionType
{
    None = 0,
    Reply = 1,
    Quote = 2,
}
