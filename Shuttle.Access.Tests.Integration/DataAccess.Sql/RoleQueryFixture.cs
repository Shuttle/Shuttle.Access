using NUnit.Framework;
using System.Transactions;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql;

public class RoleQueryFixture : DataAccessFixture
{
    [Test]
    public async Task Should_be_able_perform_all_queries_async()
    {
        var query = ServiceProvider.GetRequiredService<IRoleQuery>();

        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await Assert.ThatAsync(() => query.SearchAsync(new()), Throws.Nothing);
            Assert.That((await query.SearchAsync(new Query.Role.Specification()
                .WithTenantId(new AccessOptions().SystemTenantId)
                .AddName("Access Administrator"))).Count(), Is.LessThan(2));
        }
    }
}