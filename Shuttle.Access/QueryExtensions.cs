using Shuttle.Access.Query;
using Shuttle.Contract;

namespace Shuttle.Access;

public static class QueryExtensions
{
    extension<TModel, TSpecification>(IQuery<TModel, TSpecification> query)
        where TModel : class
        where TSpecification : Specification<TSpecification>
    {
        public async Task<bool> ContainsAsync(TSpecification specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(specification);

            specification.WithMaximumRows(1);

            return (await query.SearchAsync(specification, cancellationToken)).Any();
        }

        public async Task<TModel?> FindAsync(TSpecification specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(specification);

            specification.WithMaximumRows(2);

            var results = (await query.SearchAsync(specification, cancellationToken)).ToList();

            return results.Count > 1
                ? throw new ApplicationException($"Specification should return only a single result for '{typeof(TModel)}'.")
                : results.FirstOrDefault();
        }
    }
}