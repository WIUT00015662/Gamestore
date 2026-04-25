using Gamestore.BLL.DTOs.Comment;

namespace Gamestore.BLL.Services;

public interface ICommentService
{
    Task AddCommentAsync(string gameKey, AddCommentRequest request, Guid actorUserId, string actorName);

    Task<IEnumerable<CommentResponse>> GetCommentsByGameKeyAsync(string gameKey);

    Task UpdateCommentAsync(string gameKey, Guid commentId, UpdateCommentRequest request, Guid actorUserId, string actorName);

    Task DeleteCommentAsync(string gameKey, Guid commentId, Guid actorUserId, string actorName, bool canManageComments);

    IEnumerable<BanDurationType> GetBanDurations();

    Task BanUserAsync(BanUserRequest request);

    Task<IEnumerable<UserLookupResponse>> SearchUsersAsync(string query, int take);
}
