using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class SessionQueryFactory : ISessionQueryFactory
{
    public IQuery Contains(DataAccess.Session.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new Query($@"
IF EXISTS 
(
    SELECT 
        NULL 
    FROM 
        [dbo].[Session] s
{Where(specification)}
) 
	SELECT 1 
ELSE 
    SELECT 0
")
            .AddParameter(Columns.Token, specification.Token)
            .AddParameter(Columns.IdentityName, specification.IdentityName)
            .AddParameter(Columns.IdentityNameMatch, specification.IdentityNameMatch);
    }

    public IQuery GetPermissions(Guid identityId)
    {
        return new Query(@"
SELECT 
    p.[Id],
    p.[Name],
    p.[Description],
    p.[Status]
FROM 
	[dbo].[SessionPermission] sp
INNER JOIN
    [dbo].[Permission] p ON p.Id = sp.PermissionId
WHERE 
	IdentityId = @IdentityId
")
            .AddParameter(Columns.IdentityId, identityId);
    }

    public IQuery Save(Session session)
    {
        Guard.AgainstNull(session);

        return new Query($@"
IF NOT EXISTS (SELECT NULL FROM [dbo].[Session] WHERE IdentityName = @IdentityName)
BEGIN
    INSERT INTO [dbo].[Session] 
    (
	    Token, 
	    IdentityId, 
	    IdentityName, 
	    DateRegistered, 
	    ExpiryDate 
    )
    VALUES
    (
	    @Token, 
	    @IdentityId, 
	    @IdentityName, 
	    @DateRegistered,
        @ExpiryDate
    )
END
ELSE
BEGIN
    UPDATE
        [dbo].[Session]
    SET
        IdentityId = @IdentityId,
        Token = @Token,
        ExpiryDate = @ExpiryDate
    WHERE
        IdentityName = @IdentityName
END

DELETE FROM [dbo].[SessionPermission]
FROM 
    [dbo].[SessionPermission] sp
INNER JOIN
    [dbo].[Session] s ON s.IdentityId = sp.IdentityId
WHERE
    s.IdentityName = @IdentityName;

{(!session.HasPermissions
    ? string.Empty
    : @$"
INSERT INTO [dbo].[SessionPermission]
(
    IdentityId,
    PermissionId
)
VALUES
{string.Join(",", session.Permissions.Select(permission => $@"
(
    @IdentityId,
    '{permission.Id}'
)
"))}
")}
")
            .AddParameter(Columns.Token, session.Token)
            .AddParameter(Columns.IdentityName, session.IdentityName)
            .AddParameter(Columns.IdentityId, session.IdentityId)
            .AddParameter(Columns.DateRegistered, session.DateRegistered)
            .AddParameter(Columns.ExpiryDate, session.ExpiryDate);
    }

    public IQuery Search(DataAccess.Session.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new Query($@"
SELECT DISTINCT {(specification.MaximumRows > 0 ? $"TOP {specification.MaximumRows}" : string.Empty)}
	s.Token, 
	s.IdentityId, 
	s.IdentityName, 
	i.Description IdentityDescription, 
	s.DateRegistered, 
	s.ExpiryDate 
FROM
	[dbo].[Session] s
INNER JOIN
    [dbo].[Identity] i ON i.Id = s.IdentityId
{Where(specification)}
{(specification.MaximumRows != 1 ? "ORDER BY s.IdentityName, s.DateRegistered DESC" : string.Empty)}
")
            .AddParameter(Columns.Token, specification.Token)
            .AddParameter(Columns.IdentityName, specification.IdentityName)
            .AddParameter(Columns.IdentityId, specification.IdentityId)
            .AddParameter(Columns.IdentityNameMatch, specification.IdentityNameMatch);
    }

    public IQuery Count(DataAccess.Session.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new Query($@"
SELECT 
    COUNT(*)
FROM
	[dbo].[Session] s
{Where(specification)}
")
            .AddParameter(Columns.Token, specification.Token)
            .AddParameter(Columns.IdentityId, specification.IdentityId)
            .AddParameter(Columns.IdentityName, specification.IdentityName)
            .AddParameter(Columns.IdentityNameMatch, specification.IdentityNameMatch);
    }

    public IQuery RemoveAll()
    {
        return new Query(@"DELETE FROM [dbo].[Session]");
    }

    public IQuery Remove(Guid identityId)
    {
        return new Query(@"
DELETE 
FROM 
	[dbo].[Session] 
WHERE 
	IdentityId = @IdentityId
")
            .AddParameter(Columns.IdentityId, identityId);
    }

    private string Where(DataAccess.Session.Specification specification)
    {
        return $@"
WHERE
(
    @Token IS NULL
    OR  
	s.Token = @Token
)
AND
(
    @IdentityId IS NULL
    OR
    s.IdentityId = @IdentityId
)
AND
(
    @IdentityName IS NULL
    OR
    s.IdentityName = @IdentityName
)
AND
(
    @IdentityNameMatch IS NULL
    OR
    s.IdentityName LIKE '%' + @IdentityNameMatch + '%'
)
{(!specification.Permissions.Any() ? string.Empty : $@"
AND
    s.IdentityId IN 
    (
        SELECT DISTINCT 
            IdentityId 
        FROM 
            [dbo].[SessionPermission] sp
        INNER JOIN 
            [dbo].[Permission] p ON p.Id = sp.PermissionId
        WHERE 
            PermissionName IN ({string.Join(",", specification.Permissions.Select(item => $"'{item}'"))})
    )
")}
";
    }
}