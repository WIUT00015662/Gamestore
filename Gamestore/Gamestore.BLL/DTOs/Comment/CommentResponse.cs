namespace Gamestore.BLL.DTOs.Comment;

public class CommentResponse
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Body { get; set; }

    public bool IsDeleted { get; set; }

    public List<CommentResponse> ChildComments { get; set; } = [];
}
