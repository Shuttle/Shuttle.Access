using System;

namespace Shuttle.Access
{
    public interface ISessionRepository
    {
        void Save(Session session);
        Session Get(Guid token);
        Session Find(Guid token);
        int Remove(Guid token);
        int Remove(string username);
    }
}