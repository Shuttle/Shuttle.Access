using System;
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
            var query = new SystemRoleQuery(DatabaseGateway, QueryMapper, new SystemRoleQueryFactory());

            using(TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(() => query.Get(Guid.NewGuid()), Throws.TypeOf<RecordNotFoundException>());
                //Assert.That(() => query.Search(new SystemRoleSearchSpecification()), Throws.Nothing);
                //Assert.That(() => query.Constraints(Guid.NewGuid()), Throws.Nothing);
                //Assert.That(() => query.Operations(Guid.NewGuid()), Throws.Nothing);
            }
        }
    }
}