using System;

namespace Shuttle.Access.Sql
{
    public interface ISessionQuery
    {
        bool Contains(Guid token);
        bool Contains(Guid token, string permission);
    }
}