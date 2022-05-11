using System;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class RoleQueryFactory : IRoleQueryFactory
    {
        public IQuery Permissions(string roleName)
        {
            return RawQuery.Create(@"
select 
    Permission
from
    RolePermission rp
inner join
    Role r on (rp.RoleId = r.Id)
where
    r.RoleName = @RoleName")
                .AddParameterValue(Columns.RoleName, roleName);
        }

        public IQuery Search(DataAccess.Query.Role.Specification specification)
        {
            return Specification(specification, true);
        }

        public IQuery Added(Guid id, Added domainEvent)
        {
            return RawQuery.Create(@"
if not exists
(
    select
        null
    from
        [dbo].[Role]
    where
        RoleName = @RoleName
)
    insert into [dbo].[Role]
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
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Columns.RoleName, domainEvent.Name);
        }

        public IQuery Get(Guid id)
        {
            return RawQuery.Create(@"
select
    RoleName as Name
from
    Role
where
    Id = @Id
")
                .AddParameterValue(Columns.Id, id);
        }

        public IQuery PermissionAdded(Guid id, PermissionAdded domainEvent)
        {
            return RawQuery.Create(@"
insert into [dbo].[RolePermission]
(
	[RoleId],
	[Permission],
    [DateRegistered]
)
values
(
	@RoleId,
	@Permission,
    @DateRegistered
)
")
                .AddParameterValue(Columns.RoleId, id)
                .AddParameterValue(Columns.Permission, domainEvent.Permission)
                .AddParameterValue(Columns.DateRegistered, DateTime.UtcNow);
        }

        public IQuery PermissionRemoved(Guid id, PermissionRemoved domainEvent)
        {
            return RawQuery.Create(@"
delete 
from 
    [dbo].[RolePermission]
where	
    [RoleId] = @RoleId
and
	[Permission] = @Permission
")
                .AddParameterValue(Columns.RoleId, id)
                .AddParameterValue(Columns.Permission, domainEvent.Permission);
        }

        public IQuery Removed(Guid id)
        {
            return RawQuery.Create(@"
delete 
from 
    [dbo].[Role]
where	
    [Id] = @Id
")
                .AddParameterValue(Columns.Id, id);
        }

        public IQuery Count(DataAccess.Query.Role.Specification specification)
        {
            return Specification(specification, false);
        }

        public IQuery Permissions(DataAccess.Query.Role.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return RawQuery.Create(@"
select 
    RoleId,
    Permission
from
    RolePermission rp
inner join
    Role r on (rp.RoleId = r.Id)
where
(
    isnull(@RoleNameMatch, '') = ''
    or
    RoleName like '%' + @RoleNameMatch + '%'
)
and
(
    isnull(@RoleName, '') = ''
    or
    RoleName = @RoleName
)
and
(
    @RoleId is null
    or
    Id = @RoleId
)
and
(
    @DateRegistered is null
    or
    DateRegistered >= @DateRegistered
)
")
                .AddParameterValue(Columns.RoleNameMatch, specification.RoleNameMatch)
                .AddParameterValue(Columns.RoleName, specification.RoleName)
                .AddParameterValue(Columns.RoleId, specification.RoleId)
                .AddParameterValue(Columns.DateRegistered, specification.StartDateRegistered);

        }

        private static IQuery Specification(DataAccess.Query.Role.Specification specification, bool columns)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var what = columns
                ? @"
    Id,
    RoleName
"
                : "count(*)";

            return RawQuery.Create($@"
select
{what}
from
    Role
where
(
    isnull(@RoleNameMatch, '') = ''
    or
    RoleName like '%' + @RoleNameMatch + '%'
)
and
(
    isnull(@RoleName, '') = ''
    or
    RoleName = @RoleName
)
and
(
    @Id is null
    or
    Id = @Id
)
")
                .AddParameterValue(Columns.RoleNameMatch, specification.RoleNameMatch)
                .AddParameterValue(Columns.RoleName, specification.RoleName)
                .AddParameterValue(Columns.Id, specification.RoleId);
        }
    }
}