CREATE TABLE [dbo].[IdentityRole] (
    [IdentityId]   UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER    NOT NULL,
    [DateRegistered] DATETIMEOFFSET         CONSTRAINT [DF_IdentityRole_DateRegistered] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_IdentityRole] PRIMARY KEY CLUSTERED ([IdentityId] ASC, [RoleId] ASC)
);

