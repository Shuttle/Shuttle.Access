CREATE TABLE [dbo].[EventStore] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [Version]        INT              NOT NULL,
    [EventType]      VARCHAR (160)    NOT NULL,
    [EventEnvelope]  VARBINARY (MAX)  NOT NULL,
    [IsSnapshot]     BIT              NOT NULL,
    [SequenceNumber] BIGINT           IDENTITY (1, 1) NOT NULL,
    [DateRegistered] DATETIME         CONSTRAINT [DF_EventStore_DateRegistered] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_EventStore] PRIMARY KEY CLUSTERED ([Id] ASC, [Version] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EventStore]
    ON [dbo].[EventStore]([SequenceNumber] ASC);

