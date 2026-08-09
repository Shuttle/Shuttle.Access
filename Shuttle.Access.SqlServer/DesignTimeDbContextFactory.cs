using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shuttle.Access.SqlServer;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AccessDbContext>
{
    public AccessDbContext CreateDbContext(string[] args)
    {
        /*
            Right-click on `Shuttle.Access.SqlServer` and select `Manage User Secrets`
            {
              "ConnectionStrings": {
                "Access": "Data Source=.;Initial Catalog=Access;user id=<user>;password=<password>;TrustServerCertificate=true"
              }
            }
        */

        var connectionString = GetConnectionString(args);

        var optionsBuilder = new DbContextOptionsBuilder<AccessDbContext>();

        optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
        {
            sqlServerOptions.CommandTimeout(300);
            sqlServerOptions.MigrationsHistoryTable("__EFMigrationsHistory", "access");
        });

        return new(optionsBuilder.Options);
    }

    private static string GetConnectionString(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], "--connection", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return i + 1 >= args.Length
                ? throw new ArgumentException("Missing value for --connection.")
                : args[i + 1];
        }

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DesignTimeDbContextFactory>()
            .AddCommandLine(args)
            .Build();

        return configuration.GetConnectionString("Access")
               ?? throw new InvalidOperationException("Connection string 'Access' not found (either via --connection or configuration).");
    }
}