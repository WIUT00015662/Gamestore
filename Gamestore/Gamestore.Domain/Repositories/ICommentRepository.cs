using Gamestore.Domain.Entities;

namespace Gamestore.Domain.Repositories;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByGameIdAsync(Guid gameId);

    Task<Comment?> GetByIdWithDetailsAsync(Guid id);

    Task<IEnumerable<Comment>> GetQuotedByCommentIdAsync(Guid quotedCommentId);
}
