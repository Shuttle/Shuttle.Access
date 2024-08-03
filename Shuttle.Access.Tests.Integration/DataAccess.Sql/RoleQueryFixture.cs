using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Access.DataAccess.Query;
using Shuttle.Access.Sql;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql
{
    public class RoleQueryFixture : DataAccessFixture
    {
        [Test]
        public async Task Should_be_able_perform_all_queries_async()
        {
            var query = new RoleQuery(DatabaseGateway, QueryMapper, DataRowMapper, new RoleQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(() => query.SearchAsync(new Access.DataAccess.Query.Role.Specification()), Throws.Nothing);
                Assert.That((await query.SearchAsync(new Access.DataAccess.Query.Role.Specification().AddName("Administrator"))).Count(), Is.LessThan(2));
            }
        }
    }
}