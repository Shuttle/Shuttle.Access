using System;
using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.RestClient
{
    public interface IAccessClient
    {
        void Logout();
        void Login();

        IServerApi Server { get; }
        IPermissionsApi Permissions { get; }
        ISessionsApi Sessions { get; }
        IIdentitiesApi Identities { get; }
        Guid? Token { get; }
    }
}