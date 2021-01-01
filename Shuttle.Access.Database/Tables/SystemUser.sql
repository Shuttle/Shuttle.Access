CREATE TABLE [dbo].[SystemUser] (
    [Id]             UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Username]       VARCHAR (65)     NOT NULL,
    [DateRegistered] DATETIME         NOT NULL,
    [RegisteredBy]   VARCHAR (65)     NOT NULL,
    [GeneratedPassword] VARCHAR(65) NOT NULL DEFAULT '', 
    [DateActivated] DATETIME NULL, 
    CONSTRAINT [PK_SystemUser] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SystemUser]
    ON [dbo].[SystemUser]([Username] ASC);

