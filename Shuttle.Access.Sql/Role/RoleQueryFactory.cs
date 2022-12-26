using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class RoleQueryFactory : IRoleQueryFactory
    {
        public IQuery Search(DataAccess.Query.Role.Specification specification)
        {
            return Specification(specification, true);
        }

        public IQuery Registered(Guid id, Registered domainEvent)
        {
            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            return RawQuery.Create(@"
if not exists
(
    select
        null
    from
        [dbo].[Role]
    where
        [Name] = @Name
)
    insert into [dbo].[Role]
    (
	    [Id],
	    [Name]
    )
    values
    (
	    @Id,
	    @Name
    )
")
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Columns.Name, domainEvent.Name);
        }

        public IQuery Get(Guid id)
        {
            return RawQuery.Create(@"
select
    Name
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
	[PermissionId],
    [DateRegistered]
)
values
(
	@RoleId,
	@PermissionId,
    @DateRegistered
)
")
                .AddParameterValue(Columns.RoleId, id)
                .AddParameterValue(Columns.PermissionId, domainEvent.PermissionId)
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
	[PermissionId] = @PermissionId
")
                .AddParameterValue(Columns.RoleId, id)
                .AddParameterValue(Columns.PermissionId, domainEvent.PermissionId);
        }

        public IQuery Removed(Guid id)
        {
            return RawQuery.Create(@"
delete 
from 
    [dbo].[Role]
where	
    [Id] = @Id;

delete
from
    [dbo].[RolePermission]
where	
    [RoleId] = @Id;
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

            return RawQuery.Create($@"
select 
    rp.RoleId,
    p.Id,
    p.Name,
    p.Status
from
    RolePermission rp
inner join
    Role r on (rp.RoleId = r.Id)
inner join
    Permission p on (rp.PermissionId = p.Id)
where
(
    isnull(@NameMatch, '') = ''
    or
    r.Name like '%' + @NameMatch + '%'
)
{(!specification.Names.Any() ? string.Empty : $@"
and
    r.Name in ({string.Join(",", specification.Names.Select(item => $"'{item}'"))})
")}
{(!specification.RoleIds.Any() ? string.Empty : $@"
and
    RoleId in ({string.Join(",", specification.RoleIds.Select(item => $"'{item}'"))})
")}
{(!specification.PermissionIds.Any() ? string.Empty : $@"
and
    PermissionId in ({string.Join(",", specification.PermissionIds.Select(item => $"'{item}'"))})
")}
and
(
    @DateRegistered is null
    or
    DateRegistered >= @DateRegistered
)
order by
    p.Name
")
                .AddParameterValue(Columns.NameMatch, specification.NameMatch)
                .AddParameterValue(Columns.DateRegistered, specification.StartDateRegistered);

        }

        public IQuery NameSet(Guid id, NameSet domainEvent)
        {
            return RawQuery.Create(@"
update
    Role
set
    [Name] = @Name
where
    Id = @Id
")
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Columns.Name, domainEvent.Name);
        }

        private static IQuery Specification(DataAccess.Query.Role.Specification specification, bool columns)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var what = columns
                ? @"
    Id,
    [Name]
"
                : "count(*)";

            return RawQuery.Create($@"
select
{what}
from
    Role
where
(
    isnull(@NameMatch, '') = ''
    or
    [Name] like '%' + @NameMatch + '%'
)
{(!specification.Names.Any() ? string.Empty : $@"
and
    [Name] in ({string.Join(",", specification.Names.Select(item => $"'{item}'"))})
")}
{(!specification.RoleIds.Any() ? string.Empty : $@"
and
    Id in ({string.Join(",", specification.RoleIds.Select(item => $"'{item}'"))})
")}
{(columns ? @"
order by
    Name
" : string.Empty)}
")
                .AddParameterValue(Columns.NameMatch, specification.NameMatch);
        }
    }
}