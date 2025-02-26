CREATE TABLE [dbo].[RolePermission] (
    [RoleId]     UNIQUEIDENTIFIER NOT NULL,
    [PermissionId] UNIQUEIDENTIFIER    NOT NULL,
    [DateRegistered] DATETIMEOFFSET         CONSTRAINT [DF_RolePermission_DateRegistered] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_RolePermission] PRIMARY KEY CLUSTERED ([RoleId] ASC, [PermissionId] ASC)
);

