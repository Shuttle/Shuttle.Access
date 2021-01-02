using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public interface IRoleQuery
    {
        IEnumerable<string> Permissions(string roleName);
        IEnumerable<Query.Role> Search(Query.Role.Specification specification);
        IEnumerable<string> Permissions(Guid id);
        int Count(Query.Role.Specification specification);
    }
}