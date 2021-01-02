using System;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class IdentityQueryFactory : IIdentityQueryFactory
    {
        private const string SelectedColumns = @"
    u.Id,
    u.Name,
    u.DateRegistered,
    u.RegisteredBy
";

        public IQuery Register(Guid id, Registered domainEvent)
        {
            return RawQuery.Create(@"
insert into [dbo].[Identity]
(
	[Id],
	[Name],
	[DateRegistered],
	[RegisteredBy],
    [GeneratedPassword]
)
values
(
	@Id,
	@Name,
	@DateRegistered,
	@RegisteredBy,
    @GeneratedPassword
)
")
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Sql.Columns.Name, domainEvent.Name)
                .AddParameterValue(Sql.Columns.DateRegistered, domainEvent.DateRegistered)
                .AddParameterValue(Sql.Columns.RegisteredBy, domainEvent.RegisteredBy)
                .AddParameterValue(Sql.Columns.GeneratedPassword, domainEvent.GeneratedPassword);
        }

        public IQuery Count(DataAccess.Query.Identity.Specification specification)
        {
            return Specification(specification, false);
        }

        private IQuery Specification(DataAccess.Query.Identity.Specification specification, bool columns)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return RawQuery.Create($@"
select distinct
{(columns ? SelectedColumns : "count (*)")}
from
	[dbo].[Identity] u
left join
	IdentityRole ur on (ur.IdentityId = u.Id)
left join
	Role r on (r.Id = ur.RoleId)    
where
(
    isnull(@RoleName, '') = ''
    or
    r.RoleName = @RoleName
)
and
(
    @IdentityId is null
    or
    u.Id = @IdentityId
)
")
                .AddParameterValue(Columns.RoleName, specification.RoleName)
                .AddParameterValue(Columns.UserId, specification.IdentityId);
        }

        public IQuery RoleAdded(Guid id, RoleAdded domainEvent)
        {
            return RawQuery.Create(@"
if not exists(select null from [dbo].[IdentityRole] where IdentityId = @IdentityId and RoleId = @RoleId)
    insert into [dbo].[IdentityRole]
    (
	    [IdentityId],
	    [RoleId]
    )
    values
    (
	    @IdentityId,
	    @RoleId
    )
")
                .AddParameterValue(Columns.UserId, id)
                .AddParameterValue(Columns.RoleId, domainEvent.RoleId);
        }

        public IQuery Search(DataAccess.Query.Identity.Specification specification)
        {
            return Specification(specification, true);
        }

        public IQuery Get(Guid id)
        {
            return RawQuery.Create($@"
select
    {SelectedColumns}
from
    [dbo].[Identity] u
where 
    u.Id = @Id
")
                .AddParameterValue(Columns.Id, id);
        }

        public IQuery Roles(DataAccess.Query.Identity.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return RawQuery.Create(@"
select 
    ur.IdentityId, 
    ur.RoleId, 
    r.RoleName 
from 
    dbo.IdentityRole ur
inner join
    dbo.Role r on (r.Id = ur.RoleId)
where 
    @IdentityId is null
    or
    IdentityId = @IdentityId
")
                .AddParameterValue(Columns.UserId, specification.IdentityId);
        }

        public IQuery Roles(Guid userId)
        {
            return RawQuery.Create(@"select RoleId, RoleName from dbo.IdentityRole where IdentityId = @IdentityId")
                .AddParameterValue(Columns.UserId, userId);
        }

        public IQuery RoleRemoved(Guid id, RoleRemoved domainEvent)
        {
            return RawQuery.Create(@"
delete 
from 
    [dbo].[IdentityRole]
where
    [IdentityId] = @IdentityId
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
    dbo.IdentityRole ur
inner join
    dbo.Role r on r.Id = ur.RoleId
where 
    r.RoleName = 'administrator'
");
        }

        public IQuery RemoveRoles(Guid id, Removed domainEvent)
        {
            return RawQuery.Create("delete from IdentityRole where IdentityId = @IdentityId")
                .AddParameterValue(Columns.UserId, id);
        }

        public IQuery Remove(Guid id, Removed domainEvent)
        {
            return RawQuery.Create("delete from Identity where Id = @Id").AddParameterValue(Columns.Id, id);
        }

        public IQuery GetId(string username)
        {
            Guard.AgainstNullOrEmptyString(username, nameof(username));

            return RawQuery.Create("select Id from Identity where Name = @Name")
                .AddParameterValue(Columns.Name, username);
        }

        public IQuery Permissions(Guid id)
        {
            return RawQuery.Create(@"
select
	Permission
from
	RolePermission rp
inner join
	IdentityRole ur on ur.RoleId = rp.RoleId
where
	ur.IdentityId = @IdentityId
")
                .AddParameterValue(Columns.UserId, id);
        }

        public IQuery Activated(Guid id, Activated domainEvent)
        {
            return RawQuery.Create(@"
update
    [dbo].[Identity]
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