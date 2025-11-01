using Gamestore.Domain.Entities;

namespace Gamestore.BLL.Filters.Pipes;

public class GenreFilterPipe(List<Guid>? genreIds) : IFilterPipe
{
    public IQueryable<T> Execute<T>(IQueryable<T> query) =>
        (genreIds == null || genreIds.Count == 0) ? query :
        query.ElementType == typeof(Game) ?
            (IQueryable<T>)((IQueryable<Game>)query)
                .Where(g => g.Genres.Any(gr => genreIds.Contains(gr.Id))) :
            query;
}
