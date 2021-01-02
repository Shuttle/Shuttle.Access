using System;
using System.Linq;
using NUnit.Framework;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests.DataAccess.Sql
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
                Assert.That(() => query.Search(new Access.DataAccess.Query.Identity.Specification().WithIdentityId(new Guid("4ECABE84-D8A9-4CE3-AC40-BE3ED06DCBED")).IncludeRoles()), Throws.Nothing);
            }
        }
    }
}