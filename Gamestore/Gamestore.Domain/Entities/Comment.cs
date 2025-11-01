namespace Gamestore.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Body { get; set; }

    public Guid? ParentCommentId { get; set; }

    public Guid? QuotedCommentId { get; set; }

    public Guid GameId { get; set; }

    public bool IsDeleted { get; set; }

    public Comment? ParentComment { get; set; }

    public ICollection<Comment> ChildComments { get; set; } = [];

    public Comment? QuotedComment { get; set; }

    public ICollection<Comment> QuotedByComments { get; set; } = [];

    public Game? Game { get; set; }
}
