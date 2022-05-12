using System;
using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.RestClient
{
    public interface IAccessClient
    {
        IAccessClient Logout();
        IAccessClient Login();

        IServerApi Server { get; }
        IPermissionsApi Permissions { get; }
        ISessionsApi Sessions { get; }
        IIdentitiesApi Identities { get; }
        IRolesApi Roles { get; }
        Guid? Token { get; }
    }
}