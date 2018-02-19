using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SessionQuery : ISessionQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ISessionQueryFactory _sessionQueryFactory;

        public SessionQuery(IDatabaseGateway databaseGateway, ISessionQueryFactory sessionQueryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(sessionQueryFactory, nameof(sessionQueryFactory));

            _databaseGateway = databaseGateway;
            _sessionQueryFactory = sessionQueryFactory;
        }

        public bool Contains(Guid token)
        {
            return _databaseGateway.GetScalarUsing<int>(_sessionQueryFactory.Contains(token)) == 1;
        }

        public bool Contains(Guid token, string permission)
        {
            return _databaseGateway.GetScalarUsing<int>(_sessionQueryFactory.Contains(token, permission)) == 1;
        }
    }
}