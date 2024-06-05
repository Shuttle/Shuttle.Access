using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.RestClient
{
    public interface IAccessClient
    {
        Task<IAccessClient> DeleteSessionAsync(CancellationToken cancellationToken = default);
        Task<IAccessClient> RegisterSessionAsync(CancellationToken cancellationToken = default);

        IServerApi Server { get; }
        IPermissionsApi Permissions { get; }
        ISessionsApi Sessions { get; }
        IIdentitiesApi Identities { get; }
        IRolesApi Roles { get; }
        Guid? Token { get; }
        DateTime? TokenExpiryDate { get; }
    }
}