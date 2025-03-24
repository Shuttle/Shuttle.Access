CREATE TABLE [dbo].[Session] (
    [IdentityId]         UNIQUEIDENTIFIER NOT NULL,
    [IdentityName]       VARCHAR (65)     NOT NULL,
    [Token]          BINARY(32) NOT NULL,
    [DateRegistered] DATETIMEOFFSET         CONSTRAINT [DF_Session_DateRegistered] DEFAULT (getdate()) NOT NULL,
    [ExpiryDate]     DATETIMEOFFSET NOT NULL,
    CONSTRAINT [PK_Session] PRIMARY KEY NONCLUSTERED ([IdentityId])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Session_Token]
    ON [dbo].[Session]([Token]);


GO

CREATE UNIQUE INDEX [UX_Session_IdentityName] ON [dbo].[Session] ([IdentityName])
