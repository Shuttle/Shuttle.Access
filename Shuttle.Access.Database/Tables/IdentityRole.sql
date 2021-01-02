CREATE TABLE [dbo].[IdentityRole] (
    [IdentityId]   UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER    NOT NULL,
    CONSTRAINT [PK_IdentityRole] PRIMARY KEY CLUSTERED ([IdentityId] ASC, [RoleId] ASC)
);

