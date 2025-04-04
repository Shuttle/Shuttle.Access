﻿CREATE TABLE [dbo].[Identity] (
    [Id]             UNIQUEIDENTIFIER CONSTRAINT [DF_Identity_Id] DEFAULT (newid()) NOT NULL,
    [Name]       VARCHAR (320)     NOT NULL,
    [DateRegistered] DATETIMEOFFSET         NOT NULL,
    [RegisteredBy]   VARCHAR (320)     NOT NULL,
    [GeneratedPassword] VARCHAR(65) NULL ,
    [DateActivated] DATETIMEOFFSET NULL, 
    CONSTRAINT [PK_Identity] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Identity]
    ON [dbo].[Identity]([Name] ASC);

