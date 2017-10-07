using Shuttle.Core.Data;

namespace Shuttle.Access.Sql.Permission
{
    public class PermissionQueryFactory : IPermissionQueryFactory
    {
        public IQuery Available()
        {
            return RawQuery.Create(@"select Permission from AvailablePermission");
        }
    }
}