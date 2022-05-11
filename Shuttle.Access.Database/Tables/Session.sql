CREATE TABLE [dbo].[Session] (
    [IdentityName]       VARCHAR (65)     NOT NULL,
    [IdentityId]         UNIQUEIDENTIFIER NOT NULL,
    [Token]          UNIQUEIDENTIFIER NOT NULL,
    [DateRegistered] DATETIME2         CONSTRAINT [DF_Session_DateRegistered] DEFAULT (getdate()) NOT NULL,
    [ExpiryDate]     DATETIME2,
    CONSTRAINT [PK_Session] PRIMARY KEY NONCLUSTERED ([IdentityName] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Session]
    ON [dbo].[Session]([Token]);

