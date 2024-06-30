using System;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Sql;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql
{
    public class IdentityQueryFixture : DataAccessFixture
    {
        [Test]
        public void Should_be_able_perform_all_queries()
        {
            var query = new IdentityQuery(DatabaseGateway, QueryMapper, new IdentityQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(async () => await query.SearchAsync(new IdentitySpecification().WithIdentityId(new Guid("4ECABE84-D8A9-4CE3-AC40-BE3ED06DCBED")).IncludeRoles()), Throws.Nothing);
            }
        }
    }
}