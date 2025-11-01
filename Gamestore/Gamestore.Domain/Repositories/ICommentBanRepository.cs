using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface ICommentBanRepository : IRepository<CommentBan>
{
    Task<CommentBan?> GetByNameAsync(string name);
}
