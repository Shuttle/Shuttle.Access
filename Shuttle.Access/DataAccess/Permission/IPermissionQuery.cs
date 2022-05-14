using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQuery
    {
        IEnumerable<string> Available();
        void Register(string permission);
        void Remove(string permission);
        int Count();
    }
}