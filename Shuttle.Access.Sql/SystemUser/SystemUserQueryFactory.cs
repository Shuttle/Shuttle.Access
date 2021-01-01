using System;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.User.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemUserQueryFactory : ISystemUserQueryFactory
    {
        private const string SelectedColumns = @"
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
	[RegisteredBy],
    [GeneratedPassword]
)
values
(
	@Id,
	@Username,
	@DateRegistered,
	@RegisteredBy,
    @GeneratedPassword
)
")
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Sql.Columns.Username, domainEvent.Username)
                .AddParameterValue(Sql.Columns.DateRegistered, domainEvent.DateRegistered)
                .AddParameterValue(Sql.Columns.RegisteredBy, domainEvent.RegisteredBy)
                .AddParameterValue(Sql.Columns.GeneratedPassword, domainEvent.GeneratedPassword);
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
{(columns ? SelectedColumns : "count (*)")}
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
and
(
    @UserId is null
    or
    u.Id = @UserId
)
")
                .AddParameterValue(Columns.RoleName, specification.RoleName)
                .AddParameterValue(Columns.UserId, specification.UserId);
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
                .AddParameterValue(Columns.UserId, id)
                .AddParameterValue(Columns.RoleId, domainEvent.RoleId);
        }

        public IQuery Search(DataAccess.Query.User.Specification specification)
        {
            return Specification(specification, true);
        }

        public IQuery Get(Guid id)
        {
            return RawQuery.Create($@"
select
    {SelectedColumns}
from
    dbo.SystemUser u
where 
    u.Id = @Id
")
                .AddParameterValue(Columns.Id, id);
        }

        public IQuery Roles(DataAccess.Query.User.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return RawQuery.Create(@"
select 
    ur.UserId, 
    ur.RoleId, 
    r.RoleName 
from 
    dbo.SystemUserRole ur
inner join
    dbo.SystemRole r on (r.Id = ur.RoleId)
where 
    @UserId is null
    or
    UserId = @UserId
")
                .AddParameterValue(Columns.UserId, specification.UserId);
        }

        public IQuery Roles(Guid userId)
        {
            return RawQuery.Create(@"select RoleId, RoleName from dbo.SystemUserRole where UserId = @UserId")
                .AddParameterValue(Columns.UserId, userId);
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
                .AddParameterValue(Columns.UserId, id)
                .AddParameterValue(Columns.RoleId, domainEvent.RoleId);
        }

        public IQuery AdministratorCount()
        {
            return RawQuery.Create(@"
select 
    count(*) as count 
from 
    dbo.SystemUserRole ur
inner join
    dbo.SystemRole r on r.Id = ur.RoleId
where 
    r.RoleName = 'administrator'
");
        }

        public IQuery RemoveRoles(Guid id, Removed domainEvent)
        {
            return RawQuery.Create("delete from SystemUserRole where UserId = @UserId")
                .AddParameterValue(Columns.UserId, id);
        }

        public IQuery Remove(Guid id, Removed domainEvent)
        {
            return RawQuery.Create("delete from SystemUser where Id = @Id").AddParameterValue(Columns.Id, id);
        }

        public IQuery GetId(string username)
        {
            Guard.AgainstNullOrEmptyString(username, nameof(username));

            return RawQuery.Create("select Id from SystemUser where Username = @Username")
                .AddParameterValue(Columns.Username, username);
        }

        public IQuery Permissions(Guid id)
        {
            return RawQuery.Create(@"
select
	Permission
from
	SystemRolePermission rp
inner join
	SystemUserRole ur on ur.RoleId = rp.RoleId
where
	ur.UserId = @UserId
")
                .AddParameterValue(Columns.UserId, id);
        }

        public IQuery Activated(Guid id, Activated domainEvent)
        {
            return RawQuery.Create(@"
update
    SystemUser
set
    DateActivated = @DateActivated
where
    Id = @Id
")
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Columns.DateActivated, domainEvent.DateActivated);
        }
    }
}