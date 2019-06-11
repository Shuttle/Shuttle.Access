using System;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.User.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemUserQueryFactory : ISystemUserQueryFactory
    {
        private const string Columns = @"
    u.Id,
    u.Username,
    u.DateRegistered,
    u.RegisteredBy
";

        public IQuery Register(Guid id, Registered domainEvent)
        {
            return RawQuery.Create(@"
insert into [dbo].[SystemUser]
(
	[Id],
	[Username],
	[DateRegistered],
	[RegisteredBy]
)
values
(
	@Id,
	@Username,
	@DateRegistered,
	@RegisteredBy
)
")
                .AddParameterValue(SystemUserColumns.Id, id)
                .AddParameterValue(SystemUserColumns.Username, domainEvent.Username)
                .AddParameterValue(SystemUserColumns.DateRegistered, domainEvent.DateRegistered)
                .AddParameterValue(SystemUserColumns.RegisteredBy, domainEvent.RegisteredBy);
        }

        public IQuery Count(DataAccess.Query.User.Specification specification)
        {
            return Specification(specification, false);
        }

        private IQuery Specification(DataAccess.Query.User.Specification specification, bool columns)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return RawQuery.Create($@"
select distinct
{(columns ? Columns : "count (*)")}
from
	SystemUser u
left join
	SystemUserRole ur on (ur.UserId = u.Id)
left join
	SystemRole r on (r.Id = ur.RoleId)    
where
(
    isnull(@RoleName, '') = ''
    or
    r.RoleName = @RoleName
)
")
                .AddParameterValue(SystemRoleColumns.RoleName, specification.RoleName);
        }

        public IQuery RoleAdded(Guid id, RoleAdded domainEvent)
        {
            return RawQuery.Create(@"
if not exists(select null from [dbo].[SystemUserRole] where UserId = @UserId and RoleId = @RoleId)
    insert into [dbo].[SystemUserRole]
    (
	    [UserId],
	    [RoleId]
    )
    values
    (
	    @UserId,
	    @RoleId
    )
")
                .AddParameterValue(SystemUserRoleColumns.UserId, id)
                .AddParameterValue(SystemUserRoleColumns.RoleId, domainEvent.RoleId);
        }

        public IQuery Search(DataAccess.Query.User.Specification specification)
        {
            return Specification(specification, true);
        }

        public IQuery Get(Guid id)
        {
            return RawQuery.Create($@"
select
    {Columns}
from
    dbo.SystemUser u
where 
    u.Id = @Id
")
                .AddParameterValue(SystemUserColumns.Id, id);
        }

        public IQuery Roles(Guid id)
        {
            return RawQuery.Create(@"select RoleId from dbo.SystemUserRole where UserId = @UserId")
                .AddParameterValue(SystemUserRoleColumns.UserId, id);
        }

        public IQuery RoleRemoved(Guid id, RoleRemoved domainEvent)
        {
            return RawQuery.Create(@"
delete 
from 
    [dbo].[SystemUserRole]
where
    [UserId] = @UserId
and
	[RoleId] = @RoleId
")
                .AddParameterValue(SystemUserRoleColumns.UserId, id)
                .AddParameterValue(SystemUserRoleColumns.RoleId, domainEvent.RoleId);
        }

        public IQuery AdministratorCount()
        {
            return RawQuery.Create("select count(*) as count from dbo.SystemUserRole where RoleName = 'administrator'");
        }

        public IQuery RemoveRoles(Guid id, Removed domainEvent)
        {
            return RawQuery.Create("delete from SystemUserRole where UserId = @UserId")
                .AddParameterValue(SystemUserRoleColumns.UserId, id);
        }

        public IQuery Remove(Guid id, Removed domainEvent)
        {
            return RawQuery.Create("delete from SystemUser where Id = @Id").AddParameterValue(SystemUserColumns.Id, id);
        }
    }
}