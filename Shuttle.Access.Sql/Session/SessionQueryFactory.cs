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

    public IQuery Contains(DataAccess.Query.Session.Specification specification)
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
            .AddParameter(Columns.Token, specification.IdentityName);
    }

    public IQuery GetPermissions(Guid token)
    {
        return new Query(@"
SELECT 
	PermissionName 
FROM 
	[dbo].[SessionPermission] 
WHERE 
	Token = @Token
")
            .AddParameter(Columns.Token, token);
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

DELETE FROM [dbo].[SessionPermission]
FROM
    [dbo].[SessionPermission] sp
INNER JOIN
    [dbo].[Session] s ON s.Token = sp.Token
WHERE
    s.IdentityName = @IdentityName;

{(!session.HasPermissions
    ? string.Empty
    : @$"
INSERT INTO [dbo].[SessionPermission]
(
    Token,
    PermissionName
)
VALUES
{string.Join(",", session.Permissions.Select(permission => $@"
(
    @Token,
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

    public IQuery Remove(Guid token)
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

    public IQuery Search(DataAccess.Query.Session.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new Query($@"
SELECT DISTINCT
{SelectedColumns}
FROM
	[dbo].[Session] 
{Where(specification)}
ORDER BY
    IdentityName,
    DateRegistered DESC
")
            .AddParameter(Columns.Token, specification.Token)
            .AddParameter(Columns.IdentityName, specification.IdentityName);
    }

    public IQuery Find(Guid token)
    {
        return new Query($@"
SELECT 
{SelectedColumns}
FROM 
	[dbo].[Session] 
WHERE 
	Token = @Token
")
            .AddParameter(Columns.Token, token);
    }

    public IQuery Get(string identityName)
    {
        Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

        return new Query($@"
SELECT 
{SelectedColumns}
FROM 
	[dbo].[Session] 
WHERE 
	IdentityName = @IdentityName
")
            .AddParameter(Columns.IdentityName, identityName);
    }

    public IQuery Remove(string identityName)
    {
        Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

        return new Query(@"
DELETE 
FROM 
	[dbo].[Session] 
WHERE 
	IdentityName = @IdentityName
")
            .AddParameter(Columns.IdentityName, identityName);
    }

    private string Where(DataAccess.Query.Session.Specification specification)
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
    @IdentityName IS NULL
    OR
    IdentityName = @IdentityName
)
{(!specification.Permissions.Any() ? string.Empty : $@"
AND
    Token IN (SELECT DISTINCT Token FROM [dbo].[SessionPermission] WHERE PermissionName in ({string.Join(",", specification.Permissions.Select(item => $"'{item}'"))}))
")}
";
    }
}