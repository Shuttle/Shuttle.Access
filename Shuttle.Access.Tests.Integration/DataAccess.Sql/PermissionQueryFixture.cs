using System.Threading.Tasks;
using NUnit.Framework;
using Shuttle.Access.Sql;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql;

public class PermissionQueryFixture : DataAccessFixture
{
    [Test]
    public async Task Should_be_able_perform_all_queries()
    {
        var query = new PermissionQuery(DatabaseContextService, QueryMapper, new PermissionQueryFactory());

        using (TransactionScopeFactory.Create())
        await using (DatabaseContextFactory.Create())
        {
            await Assert.ThatAsync(() => query.SearchAsync(new Access.DataAccess.Query.Permission.Specification().AddId(new("4ECABE84-D8A9-4CE3-AC40-BE3ED06DCBED"))), Throws.Nothing);
        }
    }
}