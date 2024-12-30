using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess;

public interface ISessionQuery
{
    ValueTask<int> CountAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default); 
    ValueTask<bool> ContainsAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Messages.v1.Session>> SearchAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default);
}