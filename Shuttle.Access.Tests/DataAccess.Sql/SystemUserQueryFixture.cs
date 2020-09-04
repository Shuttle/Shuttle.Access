using System;
using System.Linq;
using NUnit.Framework;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests.DataAccess.Sql
{
    public class SystemUserQueryFixture : DataAccessFixture
    {
        [Test]
        public void Should_be_able_perform_all_queries()
        {
            var query = new SystemUserQuery(DatabaseGateway, QueryMapper, new SystemUserQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(() => query.Search(new Access.DataAccess.Query.User.Specification().WithUserId(new Guid("4ECABE84-D8A9-4CE3-AC40-BE3ED06DCBED")).IncludeRoles()), Throws.Nothing);
            }
        }
    }
}