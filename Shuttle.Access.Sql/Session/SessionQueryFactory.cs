using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class SessionQueryFactory : ISessionQueryFactory
{
    private const string SelectedColumns = @"
	Token, 
	IdentityId, 
	IdentityName, 
	DateRegistered, 
	ExpiryDate 
";

    public IQuery Contains(DataAccess.Session.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new Query($@"
IF EXISTS 
(
    SELECT 
        NULL 
    FROM 
        [dbo].[Session] 
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
	PermissionName 
FROM 
	[dbo].[SessionPermission] 
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
    {SelectedColumns}
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
        Token = @Token,
        ExpiryDate = @ExpiryDate
    WHERE
        IdentityName = @IdentityName
END

DELETE FROM 
    [dbo].[SessionPermission]
WHERE
    IdentityId = @IdentityId;

{(!session.HasPermissions
    ? string.Empty
    : @$"
INSERT INTO [dbo].[SessionPermission]
(
    IdentityId,
    PermissionName
)
VALUES
{string.Join(",", session.Permissions.Select(permission => $@"
(
    @IdentityId,
    '{permission}'
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

    public IQuery Remove(byte[] token)
    {
        return new Query(@"
DELETE 
FROM 
	[dbo].[Session] 
WHERE 
	Token = @Token
")
            .AddParameter(Columns.Token, token);
    }

    public IQuery Search(DataAccess.Session.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new Query($@"
SELECT DISTINCT {(specification.MaximumRows > 0 ? $"TOP {specification.MaximumRows}" : string.Empty)}
{SelectedColumns}
FROM
	[dbo].[Session] 
{Where(specification)}
{(specification.MaximumRows != 1 ? "ORDER BY IdentityName, DateRegistered DESC" : string.Empty)}
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
	[dbo].[Session] 
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
	Token = @Token
)
AND
(
    @IdentityId IS NULL
    OR
    IdentityId = @IdentityId
)
AND
(
    @IdentityName IS NULL
    OR
    IdentityName = @IdentityName
)
AND
(
    @IdentityNameMatch IS NULL
    OR
    IdentityName LIKE '%' + @IdentityNameMatch + '%'
)
{(!specification.Permissions.Any() ? string.Empty : $@"
AND
    Token IN (SELECT DISTINCT Token FROM [dbo].[SessionPermission] WHERE PermissionName in ({string.Join(",", specification.Permissions.Select(item => $"'{item}'"))}))
")}
";
    }
}