CREATE TABLE [dbo].[Session] (
    [IdentityName]       VARCHAR (65)     NOT NULL,
    [IdentityId]         UNIQUEIDENTIFIER NOT NULL,
    [Token]          UNIQUEIDENTIFIER NOT NULL,
    [DateRegistered] DATETIME         CONSTRAINT [DF_Session_DateRegistered] DEFAULT (getdate()) NOT NULL,
    [ExpiryDate]     DATETIME,
    CONSTRAINT [PK_Session] PRIMARY KEY NONCLUSTERED ([IdentityName] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Session]
    ON [dbo].[Session]([Token]);

