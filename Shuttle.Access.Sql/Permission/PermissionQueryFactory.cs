using Shuttle.Access.DataAccess;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class PermissionQueryFactory : IPermissionQueryFactory
    {
        public IQuery Available()
        {
            return RawQuery.Create(@"select Permission from AvailablePermission");
        }
    }
}