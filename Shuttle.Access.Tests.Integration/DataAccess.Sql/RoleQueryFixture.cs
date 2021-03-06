﻿using System;
using System.Linq;
using NUnit.Framework;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests.DataAccess.Sql
{
    public class RoleQueryFixture : DataAccessFixture
    {
        [Test]
        public void Should_be_able_perform_all_queries()
        {
            var query = new RoleQuery(DatabaseGateway, DataRowMapper, QueryMapper, new RoleQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(() => query.Search(new Access.DataAccess.Query.Role.Specification()), Throws.Nothing);
                Assert.That(
                    query.Search(new Access.DataAccess.Query.Role.Specification().WithRoleName("Administrator")).Count(),
                    Is.LessThan(2));
            }
        }
    }
}