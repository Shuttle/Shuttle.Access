using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class IdentityQuery : IIdentityQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IIdentityQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public IdentityQuery(IDatabaseGateway databaseGateway,
            IQueryMapper queryMapper, IIdentityQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));
            Guard.AgainstNull(queryMapper, nameof(queryMapper));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
            _queryMapper = queryMapper;
        }

        public async ValueTask<int> AdministratorCountAsync(CancellationToken cancellationToken = default)
        {
            return await _databaseGateway.GetScalarAsync<int>(_queryFactory.AdministratorCount(), cancellationToken);
        }

        public async ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default)
        {
            return await _databaseGateway.GetScalarAsync<Guid>(_queryFactory.GetId(identityName), cancellationToken);
        }

        public async Task<IEnumerable<string>> PermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _queryMapper.MapValuesAsync<string>(_queryFactory.Permissions(userId), cancellationToken);
        }

        public async Task<IEnumerable<Messages.v1.Identity>> SearchAsync(IdentitySpecification specification, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var result = await _queryMapper.MapObjectsAsync< Messages.v1.Identity >(_queryFactory.Search(specification), cancellationToken);

            if (specification.RolesIncluded)
            {
                var roleRows = await _databaseGateway.GetRowsAsync(_queryFactory.Roles(specification), cancellationToken);

                foreach (var roleGroup in roleRows.GroupBy(row => Columns.IdentityId.Value(row)))
                {
                    var user = result.FirstOrDefault(item => item.Id == roleGroup.Key);

                    if (user == null)
                    {
                        continue;
                    }

                    user.Roles = roleGroup.Select(row => new Messages.v1.Identity.Role
                        {Id = Columns.Id.Value(row), Name = Columns.Name.Value(row)}).ToList();
                }
            }

            return result;
        }

        public async Task<IEnumerable<Guid>> RoleIdsAsync(IdentitySpecification specification, CancellationToken cancellationToken = default)
        {
            return (await _databaseGateway.GetRowsAsync(_queryFactory.Roles(Guard.AgainstNull(specification, nameof(specification))), cancellationToken)).Select(row => Columns.RoleId.Value(row));
        }

        public async ValueTask<int> CountAsync(IdentitySpecification specification, CancellationToken cancellationToken = default)
        {
            return await _databaseGateway.GetScalarAsync<int>(_queryFactory.Count(Guard.AgainstNull(specification, nameof(specification))), cancellationToken);
        }
    }
}