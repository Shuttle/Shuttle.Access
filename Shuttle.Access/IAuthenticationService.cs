using System;

namespace Shuttle.Access
{
    public interface IAuthenticationService
    {
        AuthenticationResult Authenticate(string identityName, string password);
        AuthenticationResult Authenticate(string identityName);

        event EventHandler<AuthenticationEventArgs> Authentication;
    }
}