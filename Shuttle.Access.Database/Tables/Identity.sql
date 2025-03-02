CREATE TABLE [dbo].[Identity] (
    [Id]             UNIQUEIDENTIFIER CONSTRAINT [DF_Identity_Id] DEFAULT (newid()) NOT NULL,
    [Name]       VARCHAR (65)     NOT NULL,
    [DateRegistered] DATETIMEOFFSET         NOT NULL,
    [RegisteredBy]   VARCHAR (65)     NOT NULL,
    [GeneratedPassword] VARCHAR(65) NOT NULL CONSTRAINT [DF_Identity_GeneratedPassword] DEFAULT '',
    [DateActivated] DATETIMEOFFSET NULL, 
    CONSTRAINT [PK_Identity] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Identity]
    ON [dbo].[Identity]([Name] ASC);

