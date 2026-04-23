using Gamestore.BLL.DTOs.Comment;

namespace Gamestore.BLL.Services;

public interface ICommentService
{
    Task AddCommentAsync(string gameKey, AddCommentRequest request);

    Task<IEnumerable<CommentResponse>> GetCommentsByGameKeyAsync(string gameKey);

    Task UpdateCommentAsync(string gameKey, Guid commentId, UpdateCommentRequest request, string actorName);

    Task DeleteCommentAsync(string gameKey, Guid commentId, string actorName, bool canManageComments);

    IEnumerable<string> GetBanDurations();

    Task BanUserAsync(BanUserRequest request);

    Task<IEnumerable<string>> SearchUserNamesAsync(string query, int take);
}
