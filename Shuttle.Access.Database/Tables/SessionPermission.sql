CREATE TABLE [dbo].[SessionPermission] (
    [IdentityId]      UNIQUEIDENTIFIER NOT NULL,
    [PermissionName] VARCHAR (130)    NOT NULL,
    CONSTRAINT [PK_SessionPermission] PRIMARY KEY CLUSTERED ([IdentityId] ASC, [PermissionName] ASC),
    CONSTRAINT [FK_SessionPermission_Session] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[Session] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);

