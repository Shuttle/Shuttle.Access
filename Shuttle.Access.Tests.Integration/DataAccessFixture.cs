using System.Transactions;
using NUnit.Framework;
using Shuttle.Access.SqlServer;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Access.Tests.Integration;

[Ignore("DataAccessFixture")]
[TestFixture]
public class DataAccessFixture
{
    [SetUp]
    public void DataAccessSetUp()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DataAccessFixture>()
            .Build();

        ServiceProvider = new ServiceCollection()
            .AddAccess(accessBuilder =>
            {
                accessBuilder
                    .UseSqlServer(builder =>
                    {
                        builder.Options.ConnectionString = configuration.GetConnectionString("Access") ?? throw new ApplicationException("Missing connection string 'Access'.");
                    });
            })
            .AddTransactionScope(builder =>
            {
                builder.Configure(options =>
                {
                    options.Enabled = true;
                    options.IsolationLevel = IsolationLevel.ReadCommitted;
                    options.Timeout = TimeSpan.FromSeconds(120);
                });
            })
            .BuildServiceProvider();

        TransactionScopeFactory = ServiceProvider.GetRequiredService<ITransactionScopeFactory>();
    }

    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected ITransactionScopeFactory TransactionScopeFactory { get; private set; } = null!;
}