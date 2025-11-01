namespace Gamestore.BLL.Filters;

public interface IFilterPipe
{
    IQueryable<T> Execute<T>(IQueryable<T> query);
}
