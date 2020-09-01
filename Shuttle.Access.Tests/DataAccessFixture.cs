using System;
using System.Transactions;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Core.Transactions;
#if (NETCOREAPP2_1)
using System.Data.Common;
using System.Data.SqlClient;
#endif

namespace Shuttle.Access.Tests.DataAccess
{
    [TestFixture]
    public class DataAccessFixture
    {
        [SetUp]
        public void DataAccessSetUp()
        {
#if (NETCOREAPP2_1)
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
#endif

            TransactionScopeFactory =
                new DefaultTransactionScopeFactory(true, IsolationLevel.ReadCommitted, TimeSpan.FromSeconds(120));
            DatabaseGateway = new DatabaseGateway();
            DataRowMapper = new DataRowMapper();
            DatabaseContextCache = new ThreadStaticDatabaseContextCache();

            var connectionConfigurationProvider = new Mock<IConnectionConfigurationProvider>();

            connectionConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).Returns(
                new ConnectionConfiguration(
                    "Access",
                    "System.Data.SqlClient",
                    "Data Source=.\\sqlexpress;Initial Catalog=Access;Integrated Security=SSPI;"));

            DatabaseContextFactory = new DatabaseContextFactory(
                connectionConfigurationProvider.Object,
                new DbConnectionFactory(),
                new DbCommandFactory(),
                new ThreadStaticDatabaseContextCache());

            DatabaseContextFactory.ConfigureWith("Abacus");
            QueryMapper = new QueryMapper(DatabaseGateway, DataRowMapper);
        }

        protected ITransactionScopeFactory TransactionScopeFactory { get; private set; }
        protected IDatabaseContextCache DatabaseContextCache { get; private set; }
        protected IDatabaseGateway DatabaseGateway { get; private set; }
        protected IDatabaseContextFactory DatabaseContextFactory { get; private set; }
        protected IDataRowMapper DataRowMapper { get; private set; }
        protected IQueryMapper QueryMapper { get; private set; }
    }
}