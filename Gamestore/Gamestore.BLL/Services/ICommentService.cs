using Gamestore.BLL.DTOs.Comment;

namespace Gamestore.BLL.Services;

public interface ICommentService
{
    Task AddCommentAsync(string gameKey, AddCommentRequest request);

    Task<IEnumerable<CommentResponse>> GetCommentsByGameKeyAsync(string gameKey);

    Task DeleteCommentAsync(string gameKey, Guid commentId);

    IEnumerable<string> GetBanDurations();

    Task BanUserAsync(BanUserRequest request);
}
