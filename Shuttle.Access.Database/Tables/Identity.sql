CREATE TABLE [dbo].[Identity] (
    [Id]             UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name]       VARCHAR (65)     NOT NULL,
    [DateRegistered] DATETIME         NOT NULL,
    [RegisteredBy]   VARCHAR (65)     NOT NULL,
    [GeneratedPassword] VARCHAR(65) NOT NULL DEFAULT '', 
    [DateActivated] DATETIME NULL, 
    CONSTRAINT [PK_Identity] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Identity]
    ON [dbo].[Identity]([Name] ASC);

