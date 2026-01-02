using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Access.Data;
using System.Threading.Tasks;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql;

public class PermissionQueryFixture : DataAccessFixture
{
    [Test]
    public async Task Should_be_able_perform_all_queries()
    {
        var query = ServiceProvider.GetRequiredService<IPermissionQuery>();

        using (TransactionScopeFactory.Create())
        {
            await Assert.ThatAsync(() => query.SearchAsync(new Data.Models.Permission.Specification().AddId(new("4ECABE84-D8A9-4CE3-AC40-BE3ED06DCBED"))), Throws.Nothing);
        }
    }
}