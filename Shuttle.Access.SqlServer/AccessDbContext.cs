using Microsoft.EntityFrameworkCore;
using Shuttle.Access.SqlServer.Models;

namespace Shuttle.Access.SqlServer;

public class AccessDbContext(DbContextOptions<AccessDbContext> options) : DbContext(options)
{
    public DbSet<Models.Identity> Identities { get; set; } = null!;
    public DbSet<IdentityRole> IdentityRoles { get; set; } = null!;
    public DbSet<Models.Permission> Permissions { get; set; } = null!;
    public DbSet<Models.Role> Roles { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Models.Session> Sessions { get; set; } = null!;
    public DbSet<SessionPermission> SessionPermissions { get; set; } = null!;
    public DbSet<Models.SessionTokenExchange> SessionTokenExchange { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Identity>()
            .ToTable(nameof(Models.Identity), "access");

        modelBuilder.Entity<Models.IdentityRole>()
            .ToTable(nameof(Models.IdentityRole), "access");

        modelBuilder.Entity<Models.Permission>()
            .ToTable(nameof(Models.Permission), "access");

        modelBuilder.Entity<Models.Role>()
            .ToTable(nameof(Models.Role), "access");

        modelBuilder.Entity<Models.RolePermission>()
            .ToTable(nameof(Models.RolePermission), "access");

        modelBuilder.Entity<Models.Session>()
            .ToTable(nameof(Models.Session), "access")
            .Property(p => p.Id).HasDefaultValueSql("NEWID()");

        modelBuilder.Entity<Models.SessionPermission>()
            .ToTable(nameof(Models.SessionPermission), "access");

        modelBuilder.Entity<Models.SessionTokenExchange>()
            .ToTable(nameof(Models.SessionTokenExchange), "access");
    }
}