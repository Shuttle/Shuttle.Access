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
    i.Id,
    i.Name,
    i.DateRegistered,
    i.RegisteredBy,
    i.DateActivated,
    i.GeneratedPassword
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
                .AddParameterValue(Columns.Name, domainEvent.Name)
                .AddParameterValue(Columns.DateRegistered, domainEvent.DateRegistered)
                .AddParameterValue(Columns.RegisteredBy, domainEvent.RegisteredBy)
                .AddParameterValue(Columns.GeneratedPassword, domainEvent.GeneratedPassword);
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
	[dbo].[Identity] i
left join
	IdentityRole ir on (ir.IdentityId = i.Id)
left join
	Role r on (r.Id = ir.RoleId)    
where
(
    isnull(@Name, '') = ''
    or
    i.Name = @Name
)
and
(
    isnull(@RoleName, '') = ''
    or
    r.Name = @RoleName
)
and
(
    @IdentityId is null
    or
    i.Id = @IdentityId
)
")
                .AddParameterValue(Columns.RoleName, specification.RoleName)
                .AddParameterValue(Columns.Name, specification.Name)
                .AddParameterValue(Columns.IdentityId, specification.Id);
        }

        public IQuery RoleAdded(Guid id, RoleAdded domainEvent)
        {
            return RawQuery.Create(@"
if not exists(select null from [dbo].[IdentityRole] where IdentityId = @IdentityId and RoleId = @RoleId)
    insert into [dbo].[IdentityRole]
    (
	    [IdentityId],
	    [RoleId],
        [DateRegistered]
    )
    values
    (
	    @IdentityId,
	    @RoleId,
        @DateRegistered
    )
")
                .AddParameterValue(Columns.IdentityId, id)
                .AddParameterValue(Columns.RoleId, domainEvent.RoleId)
                .AddParameterValue(Columns.DateRegistered, DateTime.UtcNow);
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
    [dbo].[Identity] i
where 
    i.Id = @Id
")
                .AddParameterValue(Columns.Id, id);
        }

        public IQuery Roles(DataAccess.Query.Identity.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return RawQuery.Create(@"
select 
    ir.IdentityId, 
    ir.RoleId Id, 
    r.Name 
from 
    dbo.IdentityRole ir
inner join
    dbo.Role r on (r.Id = ir.RoleId)
where 
(
    @IdentityId is null
    or
    IdentityId = @IdentityId
)
and
(
    @DateRegistered is null
    or
    DateRegistered >= @DateRegistered
)
")
                .AddParameterValue(Columns.IdentityId, specification.Id)
                .AddParameterValue(Columns.DateRegistered, specification.StartDateRegistered);
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
                .AddParameterValue(Columns.IdentityId, id)
                .AddParameterValue(Columns.RoleId, domainEvent.RoleId);
        }

        public IQuery AdministratorCount()
        {
            return RawQuery.Create(@"
select 
    count(*) as count 
from 
    dbo.IdentityRole ir
inner join
    dbo.Role r on r.Id = ir.RoleId
where 
    r.Name = 'administrator'
");
        }

        public IQuery RemoveRoles(Guid id)
        {
            return RawQuery.Create("delete from IdentityRole where IdentityId = @IdentityId")
                .AddParameterValue(Columns.IdentityId, id);
        }

        public IQuery Remove(Guid id)
        {
            return RawQuery.Create("delete from [dbo].[Identity] where Id = @Id").AddParameterValue(Columns.Id, id);
        }

        public IQuery GetId(string identityName)
        {
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

            return RawQuery.Create("select Id from [dbo].[Identity] where Name = @Name")
                .AddParameterValue(Columns.Name, identityName);
        }

        public IQuery Permissions(Guid id)
        {
            return RawQuery.Create(@"
select
	Permission
from
	RolePermission rp
inner join
	IdentityRole ir on ir.RoleId = rp.RoleId
where
	ir.IdentityId = @IdentityId
")
                .AddParameterValue(Columns.IdentityId, id);
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

        public IQuery NameSet(Guid id, NameSet domainEvent)
        {
            return RawQuery.Create(@"
update
    [dbo].[Identity]
set
    [Name] = @Name
where
    Id = @Id
")
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Columns.Name, domainEvent.Name);

        }
    }
}