using Gamestore.DAL.Data;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories;

public class CommentBanRepository(GamestoreDbContext context) : Repository<CommentBan>(context), ICommentBanRepository
{
    public async Task<CommentBan?> GetByNameAsync(string name)
    {
        return await DbSet.FirstOrDefaultAsync(b => b.Name == name);
    }
}
