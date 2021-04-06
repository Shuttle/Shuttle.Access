using System;

namespace Shuttle.Access
{
    public interface ISessionRepository
    {
        void Save(Session session);
        void Renew(Session session);
        Session Get(Guid token);
        Session Find(Guid token);
        Session Find(string identityName);
        int Remove(Guid token);
        int Remove(string identityName);
    }
}