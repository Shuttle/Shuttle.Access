using System.Collections.Generic;

namespace Shuttle.Access
{
    public interface IPermissionQuery
    {
        IEnumerable<string> Available();
    }
}