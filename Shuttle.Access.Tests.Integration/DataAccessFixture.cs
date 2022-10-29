using System;
using System.Data.Common;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Core.Transactions;

namespace Shuttle.Access.Tests.Integration
{
    [TestFixture]
    public class DataAccessFixture
    {
        [SetUp]
        public void DataAccessSetUp()
        {
            DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

            TransactionScopeFactory = new TransactionScopeFactory(Options.Create(new TransactionScopeOptions
            {
                Enabled = true,
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(120)
            }));
            DatabaseContextCache = new ThreadStaticDatabaseContextCache();
            DatabaseGateway = new DatabaseGateway(DatabaseContextCache);
            DataRowMapper = new DataRowMapper();

            var connectionStringOptions = new Mock<IOptionsMonitor<ConnectionStringOptions>>();

            connectionStringOptions.Setup(m => m.Get(It.IsAny<string>())).Returns(
                (string name) => new ConnectionStringOptions
                {
                    Name = name,
                    ProviderName = "Microsoft.Data.SqlClient",
                    ConnectionString = "server=.;Initial Catalog=Access;user id=sa;password=Pass!000"
                });

            ConnectionStringOptions = connectionStringOptions.Object;

            DatabaseContextFactory = new DatabaseContextFactory(
                ConnectionStringOptions,
                Options.Create(new DataAccessOptions
                {
                    DatabaseContextFactory = new DatabaseContextFactoryOptions
                    {
                        DefaultConnectionStringName = "Access"
                    }
                }),
                new DbConnectionFactory(),
                new DbCommandFactory(Options.Create(new DataAccessOptions())),
                DatabaseContextCache);

            QueryMapper = new QueryMapper(DatabaseGateway, DataRowMapper);
        }

        protected ITransactionScopeFactory TransactionScopeFactory { get; private set; }
        protected IDatabaseContextCache DatabaseContextCache { get; private set; }
        protected IDatabaseGateway DatabaseGateway { get; private set; }
        protected IDatabaseContextFactory DatabaseContextFactory { get; private set; }
        protected IDataRowMapper DataRowMapper { get; private set; }
        protected IQueryMapper QueryMapper { get; private set; }
        protected IOptionsMonitor<ConnectionStringOptions> ConnectionStringOptions { get; private set; }
    }
}