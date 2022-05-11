CREATE TABLE [dbo].[RolePermission] (
    [RoleId]     UNIQUEIDENTIFIER NOT NULL,
    [Permission] VARCHAR (130)    NOT NULL,
    [DateRegistered] DATETIME2         CONSTRAINT [DF_RolePermission_DateRegistered] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_RolePermission] PRIMARY KEY CLUSTERED ([RoleId] ASC, [Permission] ASC)
);

