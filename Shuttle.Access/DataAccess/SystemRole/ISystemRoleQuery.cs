using System;
using System.Collections.Generic;
using Shuttle.Access.DataAccess.Query;

namespace Shuttle.Access.DataAccess
{
    public interface ISystemRoleQuery
    {
        IEnumerable<string> Permissions(string roleName);
        IEnumerable<Query.Role> Search(Query.Role.Specification specification);
        RoleExtended GetExtended(Guid id);
        IEnumerable<string> Permissions(Guid id);
        int Count(Query.Role.Specification specification);
    }
}