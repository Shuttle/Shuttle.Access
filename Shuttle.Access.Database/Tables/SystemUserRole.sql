CREATE TABLE [dbo].[SystemUserRole] (
    [UserId]   UNIQUEIDENTIFIER NOT NULL,
    [RoleName] VARCHAR (130)    NOT NULL,
    CONSTRAINT [PK_SystemUserRole] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleName] ASC)
);

