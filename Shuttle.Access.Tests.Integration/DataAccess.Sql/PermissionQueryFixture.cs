using System;
using NUnit.Framework;
using Shuttle.Access.Sql;

namespace Shuttle.Access.Tests.Integration.DataAccess.Sql
{
    public class PermissionQueryFixture : DataAccessFixture
    {
        [Test]
        public void Should_be_able_perform_all_queries()
        {
            var query = new PermissionQuery(DatabaseGateway, QueryMapper, new PermissionQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(() => query.Search(new Access.DataAccess.Query.Permission.Specification().AddId(new Guid("4ECABE84-D8A9-4CE3-AC40-BE3ED06DCBED"))), Throws.Nothing);
            }
        }
    }
}