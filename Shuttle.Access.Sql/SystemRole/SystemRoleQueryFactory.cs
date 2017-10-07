using System;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql.SystemRole
{
    public class SystemRoleQueryFactory : ISystemRoleQueryFactory
    {
        public IQuery Permissions(string roleName)
        {
            return RawQuery.Create(@"
select 
    Permission
from
    SystemRolePermission rp
inner join
    SystemRole r on (rp.RoleId = r.Id)
where
    r.RoleName = @RoleName")
                .AddParameterValue(SystemRoleColumns.RoleName, roleName);
        }

        public IQuery Permissions(Guid roleId)
        {
            return RawQuery.Create(@"
select 
    Permission
from
    SystemRolePermission
where
    RoleId = @RoleId
")
                .AddParameterValue(SystemRolePermissionColumns.RoleId, roleId);
        }

        public IQuery Search()
        {
            return RawQuery.Create(@"
select
    Id,
    RoleName
from
    SystemRole
");
        }

        public IQuery Added(Guid id, Added domainEvent)
        {
            return RawQuery.Create(@"
insert into [dbo].[SystemRole]
(
	[Id],
	[RoleName]
)
values
(
	@Id,
	@RoleName
)
")
                .AddParameterValue(SystemRoleColumns.Id, id)
                .AddParameterValue(SystemRoleColumns.RoleName, domainEvent.Name);
        }

        public IQuery Get(Guid id)
        {
            return RawQuery.Create(@"
select
    RoleName as Name
from
    SystemRole
where
    Id = @Id
")
                .AddParameterValue(SystemRoleColumns.Id, id);
        }

        public IQuery PermissionAdded(Guid id, PermissionAdded domainEvent)
        {
            return RawQuery.Create(@"
insert into [dbo].[SystemRolePermission]
(
	[RoleId],
	[Permission]
)
values
(
	@RoleId,
	@Permission
)
")
                .AddParameterValue(SystemRolePermissionColumns.RoleId, id)
                .AddParameterValue(SystemRolePermissionColumns.Permission, domainEvent.Permission);
        }

        public IQuery PermissionRemoved(Guid id, PermissionRemoved domainEvent)
        {
            return RawQuery.Create(@"
delete 
from 
    [dbo].[SystemRolePermission]
where	
    [RoleId] = @RoleId
and
	[Permission] = @Permission
")
                .AddParameterValue(SystemRolePermissionColumns.RoleId, id)
                .AddParameterValue(SystemRolePermissionColumns.Permission, domainEvent.Permission);
        }

        public IQuery Removed(Guid id, Removed domainEvent)
        {
            return RawQuery.Create(@"
delete 
from 
    [dbo].[SystemRole]
where	
    [Id] = @Id
")
                .AddParameterValue(SystemRoleColumns.Id, id);
        }
    }
}