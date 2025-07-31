using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access;

public interface IAuthorizationService
{
    Task<IEnumerable<DataAccess.Permission>> GetPermissionsAsync(string identityName, CancellationToken cancellationToken = default);
}