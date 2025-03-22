using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.RestClient;

public interface IAccessClient
{
    IIdentitiesApi Identities { get; }
    IPermissionsApi Permissions { get; }
    IRolesApi Roles { get; }
    IServerApi Server { get; }
    ISessionsApi Sessions { get; }
}