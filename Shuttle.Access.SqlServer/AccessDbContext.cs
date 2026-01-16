using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.SqlServer;

public class AccessDbContext(DbContextOptions<AccessDbContext> options) : DbContext(options)
{
    public DbSet<Models.Identity> Identities { get; set; } = null!;
    public DbSet<Models.IdentityTenant> IdentityTenants { get; set; } = null!;
    public DbSet<Models.IdentityRole> IdentityRoles { get; set; } = null!;
    public DbSet<Models.Permission> Permissions { get; set; } = null!;
    public DbSet<Models.PermissionTenant> PermissionTenants { get; set; } = null!;
    public DbSet<Models.Role> Roles { get; set; } = null!;
    public DbSet<Models.RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Models.Session> Sessions { get; set; } = null!;
    public DbSet<Models.SessionPermission> SessionPermissions { get; set; } = null!;
    public DbSet<Models.SessionTokenExchange> SessionTokenExchange { get; set; } = null!;
    public DbSet<Models.Tenant> Tenants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Session>()
            .Property(p => p.Id).HasDefaultValueSql("NEWID()");
    }
}