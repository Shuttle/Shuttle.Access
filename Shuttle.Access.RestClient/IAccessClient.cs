using System;
using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.RestClient
{
    public interface IAccessClient
    {
        IAccessClient DeleteSession();
        IAccessClient RegisterSession();

        IServerApi Server { get; }
        IPermissionsApi Permissions { get; }
        ISessionsApi Sessions { get; }
        IIdentitiesApi Identities { get; }
        IRolesApi Roles { get; }
        Guid? Token { get; }
        DateTime? TokenExpiryDate { get; }
    }
}