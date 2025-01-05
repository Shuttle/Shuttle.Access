using System;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

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
INSERT INTO [dbo].[Identity]
(
	[Id],
	[Name],
	[DateRegistered],
	[RegisteredBy],
    [GeneratedPassword]
)
VALUES
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

    public IQuery Count(DataAccess.Identity.Specification specification)
    {
        return Specification(specification, false);
    }

    public IQuery RoleAdded(Guid id, RoleAdded domainEvent)
    {
        return new Query(@"
IF NOT EXISTS(SELECT NULL FROM [dbo].[IdentityRole] WHERE IdentityId = @IdentityId AND RoleId = @RoleId)
    INSERT INTO [dbo].[IdentityRole]
    (
	    [IdentityId],
	    [RoleId],
        [DateRegistered]
    )
    VALUES
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

    public IQuery Search(DataAccess.Identity.Specification specification)
    {
        return Specification(specification, true);
    }

    public IQuery Get(Guid id)
    {
        return new Query($@"
SELECT
    {SelectedColumns}
FROM
    [dbo].[Identity] i
WHERE 
    i.Id = @Id
")
            .AddParameter(Columns.Id, id);
    }

    public IQuery Roles(DataAccess.Identity.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new Query(@"
SELECT 
    ir.IdentityId, 
    ir.RoleId Id, 
    r.Name 
FROM 
    dbo.IdentityRole ir
INNER JOIN
    dbo.Role r on (r.Id = ir.RoleId)
WHERE 
(
    @IdentityId is null
    OR
    IdentityId = @IdentityId
)
AND
(
    @DateRegistered is null
    OR
    DateRegistered >= @DateRegistered
)
")
            .AddParameter(Columns.IdentityId, specification.Id)
            .AddParameter(Columns.DateRegistered, specification.StartDateRegistered);
    }

    public IQuery RoleRemoved(Guid id, RoleRemoved domainEvent)
    {
        return new Query(@"
DELETE 
FROM 
    [dbo].[IdentityRole]
WHERE
    [IdentityId] = @IdentityId
AND
	[RoleId] = @RoleId
")
            .AddParameter(Columns.IdentityId, id)
            .AddParameter(Columns.RoleId, domainEvent.RoleId);
    }

    public IQuery AdministratorCount()
    {
        return new Query(@"
SELECT 
    count(*) as count 
FROM 
    dbo.IdentityRole ir
INNER JOIN
    dbo.Role r on r.Id = ir.RoleId
WHERE 
    r.Name = 'administrator'
");
    }

    public IQuery RemoveRoles(Guid id)
    {
        return new Query("DELETE FROM [dbo].[IdentityRole] WHERE IdentityId = @IdentityId")
            .AddParameter(Columns.IdentityId, id);
    }

    public IQuery Remove(Guid id)
    {
        return new Query("DELETE FROM [dbo].[Identity] WHERE Id = @Id").AddParameter(Columns.Id, id);
    }

    public IQuery GetId(string identityName)
    {
        Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

        return new Query("SELECT ID FROM [dbo].[Identity] WHERE Name = @Name")
            .AddParameter(Columns.Name, identityName);
    }

    public IQuery Permissions(Guid id)
    {
        return new Query(@"
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;

SELECT
	p.[Name]
FROM
	Permission p 
INNER JOIN
	RolePermission rp ON rp.PermissionId = p.Id
INNER JOIN
	IdentityRole ir ON ir.RoleId = rp.RoleId
WHERE
	ir.IdentityId = @IdentityId
AND
    p.Status <> 3
")
            .AddParameter(Columns.IdentityId, id);
    }

    public IQuery Activated(Guid id, Activated domainEvent)
    {
        return new Query(@"
UPDATE
    [dbo].[Identity]
SET
    DateActivated = @DateActivated
WHERE
    Id = @Id
")
            .AddParameter(Columns.Id, id)
            .AddParameter(Columns.DateActivated, domainEvent.DateActivated);
    }

    public IQuery NameSet(Guid id, NameSet domainEvent)
    {
        return new Query(@"
UPDATE
    [dbo].[Identity]
SET
    [Name] = @Name
WHERE
    Id = @Id
")
            .AddParameter(Columns.Id, id)
            .AddParameter(Columns.Name, domainEvent.Name);
    }

    private IQuery Specification(DataAccess.Identity.Specification specification, bool columns)
    {
        Guard.AgainstNull(specification);

        return new Query($@"
SELECT DISTINCT
{(columns ? SelectedColumns : "COUNT (DISTINCT i.Id)")}
FROM
	[dbo].[Identity] i
LEFT JOIN
	IdentityRole ir on (ir.IdentityId = i.Id)
LEFT JOIN
	Role r ON (r.Id = ir.RoleId)   
LEFT JOIN
	RolePermission rp ON (rp.RoleId = r.Id)    
LEFT JOIN
    Permission p ON (p.Id = rp.PermissionId)
WHERE
(
    ISNULL(@Name, '') = ''
    OR
    i.Name = @Name
)
AND
(
    ISNULL(@RoleName, '') = ''
    OR
    r.Name = @RoleName
)
AND
(
    @RoleId is null
    OR
    r.Id = @RoleId
)
AND
(
    @PermissionId is null
    OR
    p.Id = @PermissionId
)
AND
(
    @IdentityId is null
    OR
    i.Id = @IdentityId
)
{(columns ? @"
ORDER BY
    i.[Name]
" : string.Empty)}
")
            .AddParameter(Columns.RoleName, specification.RoleName)
            .AddParameter(Columns.Name, specification.Name)
            .AddParameter(Columns.IdentityId, specification.Id)
            .AddParameter(Columns.RoleId, specification.RoleId)
            .AddParameter(Columns.PermissionId, specification.PermissionId);
    }
}