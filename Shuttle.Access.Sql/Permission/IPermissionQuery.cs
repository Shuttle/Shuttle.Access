using System.Collections.Generic;

namespace Shuttle.Access.Sql
{
    public interface IPermissionQuery
    {
        IEnumerable<string> Available();
    }
}