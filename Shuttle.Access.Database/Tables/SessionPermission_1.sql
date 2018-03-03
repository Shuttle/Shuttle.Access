CREATE TABLE [dbo].[SessionPermission] (
    [Token]      UNIQUEIDENTIFIER NOT NULL,
    [Permission] VARCHAR (130)    NOT NULL,
    CONSTRAINT [PK_SessionPermission] PRIMARY KEY CLUSTERED ([Token] ASC, [Permission] ASC),
    CONSTRAINT [FK_SessionPermission_Session] FOREIGN KEY ([Token]) REFERENCES [dbo].[Session] ([Token]) ON DELETE CASCADE ON UPDATE CASCADE
);

