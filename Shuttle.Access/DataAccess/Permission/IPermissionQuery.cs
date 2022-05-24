using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQuery
    {
        IEnumerable<Query.Permission> Search(Query.Permission.Specification specification);
        int Count(Query.Permission.Specification specification);
        bool Contains(Query.Permission.Specification specification);
    }
}