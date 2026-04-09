using NUnit.Framework;
using Shuttle.Access.SqlServer;

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
            .BuildServiceProvider();
    }

    protected IServiceProvider ServiceProvider { get; private set; } = null!;
}