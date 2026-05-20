using Refit;
using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.RestClient;

public class AccessClient(HttpClient httpClient) : IAccessClient
{
    public IOAuthApi OAuth { get; } = RestService.For<IOAuthApi>(httpClient);
    public IIdentitiesApi Identities { get; } = RestService.For<IIdentitiesApi>(httpClient);
    public IPermissionsApi Permissions { get; } = RestService.For<IPermissionsApi>(httpClient);
    public IRolesApi Roles { get; } = RestService.For<IRolesApi>(httpClient);
    public IServerApi Server { get; } = RestService.For<IServerApi>(httpClient);
    public ISessionsApi Sessions { get; } = RestService.For<ISessionsApi>(httpClient);
}