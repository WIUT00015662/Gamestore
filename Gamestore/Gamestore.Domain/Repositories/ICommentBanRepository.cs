using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface ICommentBanRepository : IRepository<CommentBan>
{
    Task<CommentBan?> GetByUserIdAsync(Guid userId);
}
