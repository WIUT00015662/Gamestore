namespace Gamestore.BLL.Filters;

public interface IFilterPipeline
{
    IFilterPipeline AddPipe(IFilterPipe pipe);

    IQueryable<T> Execute<T>(IQueryable<T> query);
}
