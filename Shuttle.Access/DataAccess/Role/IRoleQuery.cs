using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public interface IRoleQuery
    {
        IEnumerable<Query.Role> Search(Query.Role.Specification specification);
        IEnumerable<Query.Role.RolePermission> Permissions(Query.Role.Specification specification);
        int Count(Query.Role.Specification specification);
    }
}