using System;

namespace Shuttle.Access
{
    public interface ISessionService
    {
        RegisterSessionResult Register(string identityName, Guid requesterToken);
        RegisterSessionResult Register(string identityName, string password, Guid token);
        bool Remove(Guid token);
        bool Remove(string identityName);
    }
}