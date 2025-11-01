using Gamestore.Domain.Entities;

namespace Gamestore.BLL.Filters.Pipes;

public class PaginationPipe(int pageNumber, int pageSize) : IFilterPipe
{
    public IQueryable<T> Execute<T>(IQueryable<T> query) =>
        pageSize <= 0 ? query :
        query.ElementType == typeof(Game) ?
            (IQueryable<T>)((IQueryable<Game>)query)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize) :
            query;
}
