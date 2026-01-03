IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'access')
BEGIN
    EXEC('CREATE SCHEMA access');
END
GO

IF OBJECT_ID('access.Session') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[Session];
GO

IF OBJECT_ID('access.Session') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[Session];
GO

IF OBJECT_ID('access.SessionTokenExchange') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[SessionTokenExchange];
GO

IF OBJECT_ID('access.SessionPermission') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[SessionPermission];
GO

IF OBJECT_ID('access.Role') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[Role];
GO

IF OBJECT_ID('access.RolePermission') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[RolePermission];
GO

IF OBJECT_ID('access.Identity') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[Identity];
GO

IF OBJECT_ID('access.IdentityRole') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[IdentityRole];
GO

IF OBJECT_ID('access.Permission') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[Permission];
GO

IF OBJECT_ID('access.[__EFMigrationsHistory]') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[__EFMigrationsHistory];
GO

IF OBJECT_ID('access.PrimitiveEvent') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[PrimitiveEvent];
GO

IF OBJECT_ID('access.EventType') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[EventType];
GO

IF OBJECT_ID('access.IdKey') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[IdKey];
GO

IF OBJECT_ID('access.Projection') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[Projection];
GO

IF OBJECT_ID('access.ProjectionJournal') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[ProjectionJournal];
GO

IF OBJECT_ID('access.SubscriberMessageType') IS NULL
    ALTER SCHEMA access TRANSFER dbo.[SubscriberMessageType];
GO