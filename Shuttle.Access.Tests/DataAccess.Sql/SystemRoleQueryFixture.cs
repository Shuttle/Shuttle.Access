using System;
using System.Linq;
using NUnit.Framework;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests.DataAccess.Sql
{
    public class SystemRoleQueryFixture : DataAccessFixture
    {
        [Test]
        public void Should_be_able_perform_all_queries()
        {
            var query = new SystemRoleQuery(DatabaseGateway, DataRowMapper, QueryMapper, new SystemRoleQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(() => query.Search(new Access.DataAccess.Query.Role.Specification().WithRoleId(Guid.NewGuid())), Throws.TypeOf<RecordNotFoundException>());
                Assert.That(() => query.Search(new Access.DataAccess.Query.Role.Specification()), Throws.Nothing);
                Assert.That(
                    query.Search(new Access.DataAccess.Query.Role.Specification().WithRoleName("Administrator")).Count(),
                    Is.LessThan(2));
            }
        }
    }
}