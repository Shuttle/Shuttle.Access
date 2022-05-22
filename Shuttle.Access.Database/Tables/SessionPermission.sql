CREATE TABLE [dbo].[SessionPermission] (
    [Token]      UNIQUEIDENTIFIER NOT NULL,
    [PermissionName] VARCHAR (130)    NOT NULL,
    CONSTRAINT [PK_SessionPermission] PRIMARY KEY CLUSTERED ([Token] ASC, [PermissionName] ASC),
    CONSTRAINT [FK_SessionPermission_Session] FOREIGN KEY ([Token]) REFERENCES [dbo].[Session] ([Token]) ON DELETE CASCADE ON UPDATE CASCADE
);

