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
    i.[Id],
    i.[Name],
    i.[DateRegistered],
    i.[RegisteredBy],
    i.[DateActivated],
    i.[GeneratedPassword]
";

        public IQuery Register(Guid id, Registered domainEvent)
        {
            return new Query(@"
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
                .AddParameter(Columns.Id, id)
                .AddParameter(Columns.Name, domainEvent.Name)
                .AddParameter(Columns.DateRegistered, domainEvent.DateRegistered)
                .AddParameter(Columns.RegisteredBy, domainEvent.RegisteredBy)
                .AddParameter(Columns.GeneratedPassword, domainEvent.GeneratedPassword);
        }

        public IQuery Count(IdentitySpecification specification)
        {
            return Specification(specification, false);
        }

        private IQuery Specification(IdentitySpecification specification, bool columns)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return new Query($@"
select distinct
{(columns ? SelectedColumns : "count (*)")}
from
	[dbo].[Identity] i
left join
	IdentityRole ir on (ir.IdentityId = i.Id)
left join
	Role r on (r.Id = ir.RoleId)   
left join
	RolePermission rp on (rp.RoleId = r.Id)    
left join
    Permission p on (p.Id = rp.PermissionId)
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
    @RoleId is null
    or
    r.Id = @RoleId
)
and
(
    @PermissionId is null
    or
    p.Id = @PermissionId
)
and
(
    @IdentityId is null
    or
    i.Id = @IdentityId
)
{(columns ? @"
order by
    i.[Name]
" : string.Empty)}
")
                .AddParameter(Columns.RoleName, specification.RoleName)
                .AddParameter(Columns.Name, specification.Name)
                .AddParameter(Columns.IdentityId, specification.Id)
                .AddParameter(Columns.RoleId, specification.RoleId)
                .AddParameter(Columns.PermissionId, specification.PermissionId);
        }

        public IQuery RoleAdded(Guid id, RoleAdded domainEvent)
        {
            return new Query(@"
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
                .AddParameter(Columns.IdentityId, id)
                .AddParameter(Columns.RoleId, domainEvent.RoleId)
                .AddParameter(Columns.DateRegistered, DateTime.UtcNow);
        }

        public IQuery Search(IdentitySpecification specification)
        {
            return Specification(specification, true);
        }

        public IQuery Get(Guid id)
        {
            return new Query($@"
select
    {SelectedColumns}
from
    [dbo].[Identity] i
where 
    i.Id = @Id
")
                .AddParameter(Columns.Id, id);
        }

        public IQuery Roles(IdentitySpecification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return new Query(@"
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
                .AddParameter(Columns.IdentityId, specification.Id)
                .AddParameter(Columns.DateRegistered, specification.StartDateRegistered);
        }

        public IQuery RoleRemoved(Guid id, RoleRemoved domainEvent)
        {
            return new Query(@"
delete 
from 
    [dbo].[IdentityRole]
where
    [IdentityId] = @IdentityId
and
	[RoleId] = @RoleId
")
                .AddParameter(Columns.IdentityId, id)
                .AddParameter(Columns.RoleId, domainEvent.RoleId);
        }

        public IQuery AdministratorCount()
        {
            return new Query(@"
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
            return new Query("delete from IdentityRole where IdentityId = @IdentityId")
                .AddParameter(Columns.IdentityId, id);
        }

        public IQuery Remove(Guid id)
        {
            return new Query("delete from [dbo].[Identity] where Id = @Id").AddParameter(Columns.Id, id);
        }

        public IQuery GetId(string identityName)
        {
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

            return new Query("select Id from [dbo].[Identity] where Name = @Name")
                .AddParameter(Columns.Name, identityName);
        }

        public IQuery Permissions(Guid id)
        {
            return new Query(@"
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;

select
	p.[Name]
from
	Permission p 
inner join
	RolePermission rp on rp.PermissionId = p.Id
inner join
	IdentityRole ir on ir.RoleId = rp.RoleId
where
	ir.IdentityId = @IdentityId
and
    p.Status <> 3
")
                .AddParameter(Columns.IdentityId, id);
        }

        public IQuery Activated(Guid id, Activated domainEvent)
        {
            return new Query(@"
update
    [dbo].[Identity]
set
    DateActivated = @DateActivated
where
    Id = @Id
")
                .AddParameter(Columns.Id, id)
                .AddParameter(Columns.DateActivated, domainEvent.DateActivated);
        }

        public IQuery NameSet(Guid id, NameSet domainEvent)
        {
            return new Query(@"
update
    [dbo].[Identity]
set
    [Name] = @Name
where
    Id = @Id
")
                .AddParameter(Columns.Id, id)
                .AddParameter(Columns.Name, domainEvent.Name);

        }
    }
}