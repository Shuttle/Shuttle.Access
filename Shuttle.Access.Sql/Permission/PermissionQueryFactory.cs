using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class PermissionQueryFactory : IPermissionQueryFactory
    {
        public IQuery Available()
        {
            return RawQuery.Create(@"
select 
    Permission 
from 
    AvailablePermission
");
        }

        public IQuery Register(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, nameof(permission));
            
            return RawQuery.Create(@"
if not exists
(
    select
        null
    from
        AvailablePermission
    where
        Permission = @Permission
)
    insert into
        AvailablePermission
    (
        Permission
    )
    values
    (
        @Permission
    )
")
                .AddParameterValue(Columns.Permission, permission);
        }

        public IQuery Remove(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, nameof(permission));

            return RawQuery.Create(@"
delete from
    AvailablePermission
where
    Permission = @Permission
")
                .AddParameterValue(Columns.Permission, permission);
        }
    }
}