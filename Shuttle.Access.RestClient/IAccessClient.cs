using System;
using Shuttle.Access.RestClient.v1;
using Shuttle.Access.WebApi.Models.v1;

namespace Shuttle.Access.RestClient
{
    public interface IAccessClient
    {
        void Register(string name, string password, string system);
        void Logout();
        void Login();
        void Activate(string name, DateTime dateActivate);
        void Activate(Guid id, DateTime dateActivate);
        Guid GetPasswordResetToken(string name);
        void ResetPassword(string name, Guid passwordResetToken, string password);
        RegisterSessionResponse RegisterSessionResponse { get; }
        IServerApi Server { get; }
        IPermissionsApi Permissions { get; }
        ISessionsApi Sessions { get; }
    }
}