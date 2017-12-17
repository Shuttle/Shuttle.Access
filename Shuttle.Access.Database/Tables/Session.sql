CREATE TABLE [dbo].[Session] (
    [Token]          UNIQUEIDENTIFIER NOT NULL,
    [Username]       VARCHAR (65)     NOT NULL,
    [DateRegistered] DATETIME         CONSTRAINT [DF_Session_DateRegistered] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Session] PRIMARY KEY NONCLUSTERED ([Token] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Session]
    ON [dbo].[Session]([Username] ASC);

