using System;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess;

public interface ISessionTokenExchangeQueryFactory
{
    IQuery Save(SessionTokenExchange sessionTokenExchange);
    IQuery Find(Guid exchangeToken);
    IQuery Remove(Guid exchangeToken);
    IQuery RemoveExpired();
}