CREATE TABLE [dbo].[SystemRolePermission] (
    [RoleId]     UNIQUEIDENTIFIER NOT NULL,
    [Permission] VARCHAR (130)    NOT NULL,
    CONSTRAINT [PK_SystemRolePermission] PRIMARY KEY CLUSTERED ([RoleId] ASC, [Permission] ASC)
);

