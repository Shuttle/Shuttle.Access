CREATE TABLE [dbo].[ProjectionPosition] (
    [Name]           VARCHAR (650) NOT NULL,
    [SequenceNumber] BIGINT        CONSTRAINT [DF_ProjectionPosition_SequenceNumber] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_ProjectionPosition] PRIMARY KEY NONCLUSTERED ([Name] ASC)
);

