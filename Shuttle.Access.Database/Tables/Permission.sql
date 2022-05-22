CREATE TABLE [dbo].[Permission] (
    [Id]             UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name] VARCHAR (130) NOT NULL,
    [Status] INT NOT NULL , 
    CONSTRAINT [PK_Permission] PRIMARY KEY NONCLUSTERED (Id ASC)
);


GO

CREATE CLUSTERED INDEX [IX_Permission_Name] ON [dbo].[Permission] ([Name])
