CREATE TABLE [dbo].[SessionTokenExchange]
(
	[ExchangeToken] UNIQUEIDENTIFIER NOT NULL , 
    [SessionToken] UNIQUEIDENTIFIER NOT NULL, 
    [ExpiryDate] DATETIMEOFFSET NOT NULL, 
    CONSTRAINT [PK_SessionTokenExchange] PRIMARY KEY ([ExchangeToken])
)
