using Refit;
using Shuttle.Access.RestClient.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class AccessClient : IAccessClient
{
    public AccessClient(HttpClient httpClient)
    {
        Guard.AgainstNull(httpClient);

        Server = RestService.For<IServerApi>(httpClient);
        Permissions = RestService.For<IPermissionsApi>(httpClient);
        Sessions = RestService.For<ISessionsApi>(httpClient);
        Identities = RestService.For<IIdentitiesApi>(httpClient);
        Roles = RestService.For<IRolesApi>(httpClient);
    }

    public ISessionsApi Sessions { get; }
    public IIdentitiesApi Identities { get; }
    public IRolesApi Roles { get; }
    public IServerApi Server { get; }
    public IPermissionsApi Permissions { get; }
}