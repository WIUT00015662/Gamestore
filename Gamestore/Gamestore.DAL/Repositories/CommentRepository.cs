using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories;

public class CommentRepository(GamestoreDbContext context) : Repository<Comment>(context), ICommentRepository
{
    public async Task<IEnumerable<Comment>> GetByGameIdAsync(Guid gameId)
    {
        return await DbSet
            .Where(c => c.GameId == gameId)
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<Comment?> GetByIdWithDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(c => c.Game)
            .Include(c => c.ParentComment)
            .Include(c => c.QuotedComment)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Comment>> GetQuotedByCommentIdAsync(Guid quotedCommentId)
    {
        return await DbSet
            .Where(c => c.QuotedCommentId == quotedCommentId)
            .ToListAsync();
    }
}
