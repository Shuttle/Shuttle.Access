using System;
using Shuttle.Access.RestClient.v1;
using Shuttle.Access.WebApi.Models.v1;

namespace Shuttle.Access.RestClient
{
    public interface IAccessClient
    {
        void Logout();
        void Login();
        RegisterSessionResponse RegisterSessionResponse { get; }
        IServerApi Server { get; }
        IPermissionsApi Permissions { get; }
        ISessionsApi Sessions { get; }
        IIdentitiesApi Identities { get; }
    }
}