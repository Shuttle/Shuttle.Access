using System;

namespace Shuttle.Access.DataAccess
{
    public interface ISessionQuery
    {
        bool Contains(Guid token);
        bool Contains(Guid token, string permission);
    }
}