using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Access.Data;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql;

public class RoleQueryFixture : DataAccessFixture
{
    [Test]
    public async Task Should_be_able_perform_all_queries_async()
    {
        var query = ServiceProvider.GetRequiredService<IRoleQuery>();

        using (TransactionScopeFactory.Create())
        {
            await Assert.ThatAsync(() => query.SearchAsync(new()), Throws.Nothing);
            Assert.That((await query.SearchAsync(new Data.Models.Role.Specification().AddName("Access Administrator"))).Count(), Is.LessThan(2));
        }
    }
}