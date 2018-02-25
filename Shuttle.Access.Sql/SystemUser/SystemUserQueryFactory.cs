using System;
using Shuttle.Access.Events.User.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemUserQueryFactory : ISystemUserQueryFactory
    {
        private const string SelectClause = @"
select
    Id,
    Username,
    DateRegistered,
    RegisteredBy
from
    dbo.SystemUser
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

        public IQuery Count()
        {
            return RawQuery.Create("select count(*) as count from dbo.SystemUser");
        }

        public IQuery RoleAdded(Guid id, RoleAdded domainEvent)
        {
            return RawQuery.Create(@"
if not exists(select null from [dbo].[SystemUserRole] where UserId = @UserId and RoleName = @RoleName)
    insert into [dbo].[SystemUserRole]
    (
	    [UserId],
	    [RoleName]
    )
    values
    (
	    @UserId,
	    @RoleName
    )
")
                .AddParameterValue(SystemUserRoleColumns.UserId, id)
                .AddParameterValue(SystemUserRoleColumns.RoleName, domainEvent.Role);
        }

        public IQuery Search()
        {
            return RawQuery.Create(SelectClause);
        }

        public IQuery Get(Guid id)
        {
            return RawQuery.Create(string.Concat(SelectClause, " where Id = @Id"))
                .AddParameterValue(SystemUserColumns.Id, id);
        }

        public IQuery Roles(Guid id)
        {
            return RawQuery.Create(@"select RoleName from dbo.SystemUserRole where UserId = @UserId")
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
	[RoleName] = @RoleName
")
                .AddParameterValue(SystemUserRoleColumns.UserId, id)
                .AddParameterValue(SystemUserRoleColumns.RoleName, domainEvent.Role);
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