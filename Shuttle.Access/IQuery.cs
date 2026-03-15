namespace Shuttle.Access;

public interface IQuery<TModel, in TSpecification>
    where TModel : class
    where TSpecification : class
{
    Task<IEnumerable<TModel>> SearchAsync(TSpecification specification, CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(TSpecification specification, CancellationToken cancellationToken = default);
}