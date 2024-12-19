using System;
using System.Data.Common;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Core.Transactions;

namespace Shuttle.Access.Tests.Integration;

[TestFixture]
public class DataAccessFixture
{
    [SetUp]
    public void DataAccessSetUp()
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

        var serviceProvider = new ServiceCollection()
            .AddDataAccess(builder =>
            {
                builder.AddConnectionString("Access", "Microsoft.Data.SqlClient", "server=.;database=Access;user id=sa;password=Pass!000;TrustServerCertificate=true");
                builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
            })
            .AddTransactionScope(builder =>
            {
                builder.Options.Enabled = true;
                builder.Options.IsolationLevel = IsolationLevel.ReadCommitted;
                builder.Options.Timeout = TimeSpan.FromSeconds(120);
            })
            .BuildServiceProvider();

        DatabaseContextService = serviceProvider.GetRequiredService<IDatabaseContextService>();
        DataRowMapper = serviceProvider.GetRequiredService<IDataRowMapper>();
        QueryMapper = serviceProvider.GetRequiredService<IQueryMapper>();
        DatabaseContextFactory = serviceProvider.GetRequiredService<IDatabaseContextFactory>();
        TransactionScopeFactory = serviceProvider.GetRequiredService<ITransactionScopeFactory>();
    }

    protected ITransactionScopeFactory TransactionScopeFactory { get; private set; } = null!;
    protected IDatabaseContextService DatabaseContextService { get; private set; } = null!;
    protected IDatabaseContextFactory DatabaseContextFactory { get; private set; } = null!;
    protected IDataRowMapper DataRowMapper { get; private set; } = null!;
    protected IQueryMapper QueryMapper { get; private set; } = null!;
}