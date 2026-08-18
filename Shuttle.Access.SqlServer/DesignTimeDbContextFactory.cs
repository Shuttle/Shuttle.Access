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

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DesignTimeDbContextFactory>()
            .Build();

        // A real connection string is only required for local `dotnet ef migrations add`.
        // For `database update`/migrations bundles, EF replaces this via `--connection` after
        // the context has been constructed, so a placeholder here is enough to avoid failing
        // context creation when no user secrets are configured (e.g. inside a container).
        var connectionString = configuration.GetConnectionString("Access")
                                ?? "Data Source=.;Initial Catalog=Access;Integrated Security=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<AccessDbContext>();

        optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
        {
            sqlServerOptions.CommandTimeout(300);
            sqlServerOptions.MigrationsHistoryTable("__EFMigrationsHistory", "access");
        });

        return new(optionsBuilder.Options);
    }
}