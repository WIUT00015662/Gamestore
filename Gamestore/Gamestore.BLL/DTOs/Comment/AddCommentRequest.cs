using System.Text.Json.Serialization;
using Gamestore.BLL.Serialization;

namespace Gamestore.BLL.DTOs.Comment;

public class AddCommentRequest
{
    public required CommentBodyDto Comment { get; set; }

    [JsonConverter(typeof(NullableGuidEmptyStringJsonConverter))]
    public Guid? ParentId { get; set; }

    public string? Action { get; set; }
}
