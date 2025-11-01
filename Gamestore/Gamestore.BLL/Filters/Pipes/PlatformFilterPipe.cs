using Gamestore.Domain.Entities;

namespace Gamestore.BLL.Filters.Pipes;

public class PlatformFilterPipe(List<Guid>? platformIds) : IFilterPipe
{
    public IQueryable<T> Execute<T>(IQueryable<T> query) =>
        (platformIds == null || platformIds.Count == 0) ? query :
        query.ElementType == typeof(Game) ?
            (IQueryable<T>)((IQueryable<Game>)query)
                .Where(g => g.Platforms.Any(p => platformIds.Contains(p.Id))) :
            query;
}
