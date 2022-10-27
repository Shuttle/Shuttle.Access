using System;

namespace Shuttle.Access.Tests.Integration
{
    public class SessionService : ISessionService
    {
        public RegisterSessionResult Register(string identityName, Guid requesterToken)
        {
            throw new NotImplementedException();
        }

        public RegisterSessionResult Register(string identityName, string password, Guid token)
        {
            return RegisterSessionResult.Success(identityName ?? "identity", Guid.Empty.Equals(token) ? Guid.NewGuid() : token, DateTime.MaxValue, new[] { "*" });
        }

        public bool Remove(Guid token)
        {
            return true;
        }

        public bool Remove(string identityName)
        {
            return true;
        }

        public void Refresh(Guid token)
        {
            
        }

        public event EventHandler<SessionOperationEventArgs> SessionOperation = delegate
        {
        };
    }
}