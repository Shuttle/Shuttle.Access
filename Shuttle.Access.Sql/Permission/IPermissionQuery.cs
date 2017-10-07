using System.Collections.Generic;

namespace Shuttle.Access.Sql.Permission
{
    public interface IPermissionQuery
    {
        IEnumerable<string> Available();
    }
}