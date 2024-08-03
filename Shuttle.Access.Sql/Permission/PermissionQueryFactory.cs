using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.DataAccess.Query;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class PermissionQueryFactory : IPermissionQueryFactory
    {
        public IQuery Search(DataAccess.Query.Permission.Specification specification)
        {
            return Specification(specification, true);
        }

        public IQuery Count(DataAccess.Query.Permission.Specification specification)
        {
            return Specification(specification, false);
        }

        public IQuery Specification(DataAccess.Query.Permission.Specification specification, bool columns)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var what = columns
                ? @"
    p.[Id],
    p.[Name],
    p.[Status]
"
                : "count(*)";

            return new Query($@"
select distinct
{what}
from
    Permission p
{(!specification.RoleIds.Any() ? string.Empty : @"
inner join
    RolePermission rp on (rp.PermissionId = p.Id)
")}
{Where(specification)}
")
                .AddParameter(Columns.NameMatch, specification.NameMatch);
        }

        private string Where(DataAccess.Query.Permission.Specification specification)
        {
            return $@"
where
(
    isnull(@NameMatch, '') = ''
    or
    p.[Name] like '%' + @NameMatch + '%'
)
{(!specification.Names.Any() ? string.Empty : $@"
and
    p.[Name] in ({string.Join(",", specification.Names.Select(item => $"'{item}'"))})
")}
{(!specification.Ids.Any() ? string.Empty : $@"
and
    p.Id in ({string.Join(",", specification.Ids.Select(item => $"'{item}'"))})
")}
{(!specification.RoleIds.Any() ? string.Empty : $@"
and
    rp.RoleId in ({string.Join(",", specification.RoleIds.Select(item => $"'{item}'"))})
")}
";
        }

        public IQuery Registered(Guid id, Registered domainEvent)
        {
            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            return new Query(@"
if not exists
(
    select
        null
    from
        [dbo].[Permission]
    where
        [Name] = @Name
)
    insert into [dbo].[Permission]
    (
	    [Id],
	    [Name],
        [Status]
    )
    values
    (
	    @Id,
	    @Name,
        @Status
    )
")
                .AddParameter(Columns.Id, id)
                .AddParameter(Columns.Name, domainEvent.Name)
                .AddParameter(Columns.Status, (int)domainEvent.Status);
        }

        public IQuery Activated(Guid id, Activated domainEvent)
        {
            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            return SetStatus(id, (int)PermissionStatus.Active);
        }

        private static IQuery SetStatus(Guid id, int status)
        {
            return new Query(@"
update
    Permission
set
    Status = @Status
where
    Id = @Id
")
                .AddParameter(Columns.Id, id)
                .AddParameter(Columns.Status, status);
        }

        public IQuery Deactivated(Guid id, Deactivated domainEvent)
        {
            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            return SetStatus(id, (int)PermissionStatus.Deactivated);
        }

        public IQuery Removed(Guid id, Removed domainEvent)
        {
            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            return SetStatus(id, (int)PermissionStatus.Removed);
        }

        public IQuery Contains(DataAccess.Query.Permission.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return new Query($@"
if exists
(
select
    null
from
    Permission p
{Where(specification)}
)
    select 1
else
    select 0
")
                .AddParameter(Columns.NameMatch, specification.NameMatch);
        }

        public IQuery NameSet(Guid id, NameSet domainEvent)
        {
            return new Query(@"
update
    Permission
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