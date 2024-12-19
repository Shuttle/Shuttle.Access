using System.Threading.Tasks;
using NUnit.Framework;
using Shuttle.Access.Sql;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql;

public class IdentityQueryFixture : DataAccessFixture
{
    [Test]
    public async Task Should_be_able_perform_all_queries_async()
    {
        var query = new IdentityQuery(DatabaseContextService, QueryMapper, new IdentityQueryFactory());

        using (TransactionScopeFactory.Create())
        await using (DatabaseContextFactory.Create())
        {
            await Assert.ThatAsync(async () => await query.SearchAsync(new Access.DataAccess.Query.Identity.Specification().WithIdentityId(new("4ECABE84-D8A9-4CE3-AC40-BE3ED06DCBED")).IncludeRoles()), Throws.Nothing);
        }
    }
}