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
            .AddAccess()
            .UseSqlServer(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("Access") ?? throw new ApplicationException("Missing connection string 'Access'.");
            })
            .Services
            .BuildServiceProvider();
    }

    protected IServiceProvider ServiceProvider { get; private set; } = null!;
}