using System;
using System.Linq;
using Shuttle.Access.DataAccess;
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
    Id,
    [Name],
    [Status]
"
                : "count(*)";

            return RawQuery.Create($@"
select
{what}
from
    Permission
{Where(specification)}
")
                .AddParameterValue(Columns.NameMatch, specification.NameMatch);
        }

        private string Where(DataAccess.Query.Permission.Specification specification)
        {
            return $@"
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
{(!specification.Ids.Any() ? string.Empty : $@"
and
    Id in ({string.Join(",", specification.Ids.Select(item => $"'{item}'"))})
")}";
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
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Columns.Name, domainEvent.Name)
                .AddParameterValue(Columns.Status, (int)domainEvent.Status);
        }

        public IQuery Activated(Guid id, Activated domainEvent)
        {
            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            return SetStatus(id, (int)PermissionStatus.Active);
        }

        private static IQuery SetStatus(Guid id, int status)
        {
            return RawQuery.Create(@"
update
    Permission
set
    Status = @Status
where
    Id = @Id
")
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Columns.Status, status);
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

            return RawQuery.Create($@"
if exists
(
select
    null
from
    Permission
{Where(specification)}
)
    select 1
else
    select 0
")
                .AddParameterValue(Columns.NameMatch, specification.NameMatch);
        }

        public IQuery NameSet(Guid id, NameSet domainEvent)
        {
            return RawQuery.Create(@"
update
    Permission
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