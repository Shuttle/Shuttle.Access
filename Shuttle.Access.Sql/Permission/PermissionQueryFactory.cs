using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class PermissionQueryFactory : IPermissionQueryFactory
{
    public IQuery Search(DataAccess.Permission.Specification specification)
    {
        return Specification(specification, true);
    }

    public IQuery Count(DataAccess.Permission.Specification specification)
    {
        return Specification(specification, false);
    }

    public IQuery Registered(Guid id, Registered domainEvent)
    {
        Guard.AgainstNull(domainEvent);

        return new Query(@"
IF NOT EXISTS
(
    SELECT
        NULL
    FROM
        [dbo].[Permission]
    WHERE
        [Name] = @Name
)
    INSERT INTO [dbo].[Permission]
    (
	    [Id],
	    [Name],
	    [Description],
        [Status]
    )
    VALUES
    (
	    @Id,
	    @Name,
        @Description,
        @Status
    )
")
            .AddParameter(Columns.Id, id)
            .AddParameter(Columns.Name, domainEvent.Name)
            .AddParameter(Columns.Description, domainEvent.Description)
            .AddParameter(Columns.Status, (int)domainEvent.Status);
    }

    public IQuery Activated(Guid id, Activated domainEvent)
    {
        Guard.AgainstNull(domainEvent);

        return SetStatus(id, (int)PermissionStatus.Active);
    }

    public IQuery Deactivated(Guid id, Deactivated domainEvent)
    {
        Guard.AgainstNull(domainEvent);

        return SetStatus(id, (int)PermissionStatus.Deactivated);
    }

    public IQuery Removed(Guid id, Removed domainEvent)
    {
        Guard.AgainstNull(domainEvent);

        return SetStatus(id, (int)PermissionStatus.Removed);
    }

    public IQuery Contains(DataAccess.Permission.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new Query($@"
IF EXISTS
(
    SELECT
        NULL
    FROM
        [dbo].[Permission] p
{Where(specification)}
)
    SELECT 1
ELSE
    SELECT 0
")
            .AddParameter(Columns.NameMatch, specification.NameMatch);
    }

    public IQuery NameSet(Guid id, NameSet domainEvent)
    {
        return new Query(@"
UPDATE
    [dbo].[Permission]
SET
    [Name] = @Name
WHERE
    Id = @Id
")
            .AddParameter(Columns.Id, id)
            .AddParameter(Columns.Name, domainEvent.Name);
    }

    public IQuery DescriptionSet(Guid id, DescriptionSet domainEvent)
    {
        return new Query(@"
UPDATE
    [dbo].[Permission]
SET
    [Description] = @Description
WHERE
    Id = @Id
")
            .AddParameter(Columns.Id, id)
            .AddParameter(Columns.Description, domainEvent.Description);
    }

    private static IQuery SetStatus(Guid id, int status)
    {
        return new Query(@"
UPDATE
    [dbo].[Permission]
SET
    Status = @Status
WHERE
    Id = @Id
")
            .AddParameter(Columns.Id, id)
            .AddParameter(Columns.Status, status);
    }

    public IQuery Specification(DataAccess.Permission.Specification specification, bool columns)
    {
        Guard.AgainstNull(specification);

        var what = columns
            ? @"
    p.[Id],
    p.[Name],
    p.[Description],
    p.[Status]
"
            : "count(*)";

        return new Query($@"
SELECT DISTINCT {(columns && specification.MaximumRows > 0 ? $"TOP {specification.MaximumRows}" : string.Empty)}
{what}
FROM
    [dbo].[Permission] p
{(!specification.RoleIds.Any() ? string.Empty : @"
INNER JOIN
    [dbo].[RolePermission] rp ON (rp.PermissionId = p.Id)
")}
{Where(specification)}
{(columns ? "ORDER BY p.[Name]" : string.Empty)}
")
            .AddParameter(Columns.NameMatch, specification.NameMatch);
    }

    private string Where(DataAccess.Permission.Specification specification)
    {
        return $@"
WHERE
(
    isnull(@NameMatch, '') = ''
    OR
    p.[Name] LIKE '%' + @NameMatch + '%'
    OR
    p.Description LIKE '%' + @NameMatch + '%'
)
{(!specification.Names.Any() ? string.Empty : $@"
AND
(
    p.[Name] IN ({string.Join(",", specification.Names.Select(item => $"'{item}'"))})
    OR
    p.[Description] IN ({string.Join(",", specification.Names.Select(item => $"'{item}'"))})
)
")}
{(!specification.Ids.Any() ? string.Empty : $@"
AND
    p.Id IN ({string.Join(",", specification.Ids.Select(item => $"'{item}'"))})
")}
{(!specification.RoleIds.Any() ? string.Empty : $@"
AND
    rp.RoleId IN ({string.Join(",", specification.RoleIds.Select(item => $"'{item}'"))})
")}
";
    }
}