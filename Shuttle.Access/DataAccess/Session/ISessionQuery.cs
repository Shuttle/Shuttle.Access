using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public interface ISessionQuery
    {
        bool Contains(Guid token);
        bool Contains(Guid token, string permission);
        Query.Session Get(Guid token);
        IEnumerable<Query.Session> Search(Query.Session.Specification specification);
    }
}