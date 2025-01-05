using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shuttle.Access.Sql;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql;

public class RoleQueryFixture : DataAccessFixture
{
    [Test]
    public async Task Should_be_able_perform_all_queries_async()
    {
        var query = new RoleQuery(DatabaseContextService, QueryMapper, DataRowMapper, new RoleQueryFactory());

        using (TransactionScopeFactory.Create())
        await using (DatabaseContextFactory.Create())
        {
            await Assert.ThatAsync(() => query.SearchAsync(new()), Throws.Nothing);
            Assert.That((await query.SearchAsync(new Access.DataAccess.Role.Specification().AddName("Administrator"))).Count(), Is.LessThan(2));
        }
    }
}