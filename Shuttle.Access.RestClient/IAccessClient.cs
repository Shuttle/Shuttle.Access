using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.RestClient;

public interface IAccessClient
{
    IIdentitiesApi Identities { get; }
    IPermissionsApi Permissions { get; }
    IRolesApi Roles { get; }

    IServerApi Server { get; }
    ISessionsApi Sessions { get; }
    Guid? Token { get; }
    DateTimeOffset? TokenExpiryDate { get; }
    Task<IAccessClient> DeleteSessionAsync(CancellationToken cancellationToken = default);
    Task<IAccessClient> RegisterSessionAsync(CancellationToken cancellationToken = default);
}