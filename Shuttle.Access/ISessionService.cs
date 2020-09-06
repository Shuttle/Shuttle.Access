using System;

namespace Shuttle.Access
{
    public interface ISessionService
    {
        RegisterSessionResult Register(string username, string password, Guid token);
        RegisterSessionResult Register(string username, Guid token);
        bool Remove(Guid token);
        bool Remove(string username);
    }
}