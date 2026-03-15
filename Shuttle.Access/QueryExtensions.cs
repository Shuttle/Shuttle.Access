using Shuttle.Core.Contract;

namespace Shuttle.Access;

public static class QueryExtensions
{
    extension<TModel, TSpecification>(IQuery<TModel, TSpecification> query)
        where TModel : class
        where TSpecification : class
    {
        public async ValueTask<bool> ContainsAsync(TSpecification specification, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(query).CountAsync(specification, cancellationToken) > 0;
        }
    }
}