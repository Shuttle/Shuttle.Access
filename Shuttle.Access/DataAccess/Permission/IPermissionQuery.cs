using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQuery
    {
        IEnumerable<string> Available();
    }
}