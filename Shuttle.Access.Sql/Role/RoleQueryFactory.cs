using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class RoleQueryFactory : IRoleQueryFactory
{
    public IQuery Search(DataAccess.Role.Specification specification)
    {
        return Specification(specification, true);
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
        [dbo].[Role]
    WHERE
        [Name] = @Name
)
    INSERT INTO [dbo].[Role]
    (
	    [Id],
	    [Name]
    )
    VALUES
    (
	    @Id,
	    @Name
    )
")
            .AddParameter(Columns.Id, id)
            .AddParameter(Columns.Name, domainEvent.Name);
    }

    public IQuery Get(Guid id)
    {
        return new Query(@"
SELECT
    Name
FROM
    Role
WHERE
    Id = @Id
")
            .AddParameter(Columns.Id, id);
    }

    public IQuery PermissionAdded(Guid id, PermissionAdded domainEvent)
    {
        return new Query(@"
INSERT INTO [dbo].[RolePermission]
(
	[RoleId],
	[PermissionId],
    [DateRegistered]
)
VALUES
(
	@RoleId,
	@PermissionId,
    @DateRegistered
)
")
            .AddParameter(Columns.RoleId, id)
            .AddParameter(Columns.PermissionId, domainEvent.PermissionId)
            .AddParameter(Columns.DateRegistered, DateTimeOffset.UtcNow);
    }

    public IQuery PermissionRemoved(Guid id, PermissionRemoved domainEvent)
    {
        return new Query(@"
DELETE 
FROM 
    [dbo].[RolePermission]
WHERE	
    [RoleId] = @RoleId
AND
	[PermissionId] = @PermissionId
")
            .AddParameter(Columns.RoleId, id)
            .AddParameter(Columns.PermissionId, domainEvent.PermissionId);
    }

    public IQuery Removed(Guid id)
    {
        return new Query(@"
DELETE 
FROM 
    [dbo].[Role]
WHERE	
    [Id] = @Id;

DELETE
FROM
    [dbo].[RolePermission]
WHERE	
    [RoleId] = @Id;
")
            .AddParameter(Columns.Id, id);
    }

    public IQuery Count(DataAccess.Role.Specification specification)
    {
        return Specification(specification, false);
    }

    public IQuery Permissions(DataAccess.Role.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new Query($@"
SELECT {(specification.MaximumRows > 0 ? $"TOP {specification.MaximumRows}" : string.Empty)}
    rp.RoleId,
    p.Id,
    p.Name,
    p.Description,
    p.Status
FROM
    RolePermission rp
INNER JOIN
    Role r ON (rp.RoleId = r.Id)
INNER JOIN
    Permission p ON (rp.PermissionId = p.Id)
WHERE
(
    isnull(@NameMatch, '') = ''
    OR
    r.Name LIKE '%' + @NameMatch + '%'
)
{(!specification.Names.Any() ? string.Empty : $@"
AND
    r.Name IN ({string.Join(",", specification.Names.Select(item => $"'{item}'"))})
")}
{(!specification.RoleIds.Any() ? string.Empty : $@"
AND
    RoleId IN ({string.Join(",", specification.RoleIds.Select(item => $"'{item}'"))})
")}
{(!specification.PermissionIds.Any() ? string.Empty : $@"
AND
    PermissionId IN ({string.Join(",", specification.PermissionIds.Select(item => $"'{item}'"))})
")}
AND
(
    @DateRegistered IS NULL
    OR
    DateRegistered >= @DateRegistered
)
ORDER BY
    p.Name
")
            .AddParameter(Columns.NameMatch, specification.NameMatch)
            .AddParameter(Columns.DateRegistered, specification.StartDateRegistered);
    }

    public IQuery NameSet(Guid id, NameSet domainEvent)
    {
        return new Query(@"
UPDATE
    Role
SET
    [Name] = @Name
WHERE
    Id = @Id
")
            .AddParameter(Columns.Id, id)
            .AddParameter(Columns.Name, domainEvent.Name);
    }

    private static IQuery Specification(DataAccess.Role.Specification specification, bool columns)
    {
        Guard.AgainstNull(specification);

        var what = columns
            ? @"Id, [Name]"
            : "COUNT(*)";

        return new Query($@"
SELECT {(columns && specification.MaximumRows > 0 ? $"TOP {specification.MaximumRows}" : string.Empty)}
{what}
FROM
    Role
WHERE
(
    ISNULL(@NameMatch, '') = ''
    OR
    [Name] LIKE '%' + @NameMatch + '%'
)
{(!specification.Names.Any() ? string.Empty : $@"
AND
    [Name] IN ({string.Join(",", specification.Names.Select(item => $"'{item}'"))})
")}
{(!specification.RoleIds.Any() ? string.Empty : $@"
AND
    Id IN ({string.Join(",", specification.RoleIds.Select(item => $"'{item}'"))})
")}
{(columns ? @"
ORDER BY
    Name
" : string.Empty)}
")
            .AddParameter(Columns.NameMatch, specification.NameMatch);
    }
}