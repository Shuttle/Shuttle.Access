CREATE TABLE [dbo].[EventStore] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [Version]        INT              NOT NULL,
    [EventEnvelope]  VARBINARY (MAX)  NOT NULL,
    [EventId]        UNIQUEIDENTIFIER NOT NULL,
    [EventTypeId]    UNIQUEIDENTIFIER NOT NULL,
    [IsSnapshot]     BIT              NOT NULL,
    [SequenceNumber] BIGINT           IDENTITY (1, 1) NOT NULL,
    [DateRegistered] DATETIME2 (7)    CONSTRAINT [DF_EventStore_DateRegistered] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_EventStore] PRIMARY KEY CLUSTERED ([Id] ASC, [Version] ASC),
    CONSTRAINT [FK_EventStore_EventType] FOREIGN KEY ([EventTypeId]) REFERENCES [dbo].[EventType] ([Id])
);






GO
CREATE NONCLUSTERED INDEX [IX_EventStore]
    ON [dbo].[EventStore]([SequenceNumber] ASC);

