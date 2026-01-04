using NUnit.Framework;
using Shuttle.Access.SqlServer;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql;

public class IdentityQueryFixture : DataAccessFixture
{
    [Test]
    public async Task Should_be_able_perform_all_queries_async()
    {
        var query = ServiceProvider.GetRequiredService<IIdentityQuery>();

        using (TransactionScopeFactory.Create())
        {
            await Assert.ThatAsync(async () => await query.SearchAsync(new SqlServer.Models.Identity.Specification().WithIdentityId(new("4ECABE84-D8A9-4CE3-AC40-BE3ED06DCBED")).IncludeRoles()), Throws.Nothing);
        }
    }
}