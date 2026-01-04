using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shuttle.Access.SqlServer;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AccessDbContext>
{
    public AccessDbContext CreateDbContext(string[] args)
    {
        /*
            Right-click on `Shuttle.Access.Data` and select `Manage User Secrets`
            {
              "ConnectionStrings": {
                "Access": "Data Source=.;Initial Catalog=Access;user id=<user>;password=<password>;TrustServerCertificate=true"
              }
            }
        */

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DesignTimeDbContextFactory>()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AccessDbContext>();

        optionsBuilder.UseSqlServer(configuration.GetConnectionString("Access"));

        return new(optionsBuilder.Options);
    }
}