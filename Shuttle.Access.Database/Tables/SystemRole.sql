CREATE TABLE [dbo].[SystemRole] (
    [Id]       UNIQUEIDENTIFIER NOT NULL,
    [RoleName] VARCHAR (130)    NOT NULL,
    CONSTRAINT [PK_SystemRole] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SystemRole]
    ON [dbo].[SystemRole]([RoleName] ASC);

