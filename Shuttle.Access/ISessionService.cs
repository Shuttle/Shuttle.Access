using System;

namespace Shuttle.Access
{
    public interface ISessionService
    {
        RegisterSessionResult Register(string username, string password, Guid token);
    }
}