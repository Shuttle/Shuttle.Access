using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shuttle.Access.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AccessDbContext>
{
    public AccessDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DesignTimeDbContextFactory>()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AccessDbContext>();
        
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("Access"),
            builder => builder.MigrationsHistoryTable("__EFMigrationsHistory"));

        return new(optionsBuilder.Options);
    }
}