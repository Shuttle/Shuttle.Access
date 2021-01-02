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
    Permission
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
        Permission
    where
        Permission = @Permission
)
    insert into
        Permission
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
    Permission
where
    Permission = @Permission
")
                .AddParameterValue(Columns.Permission, permission);
        }
    }
}