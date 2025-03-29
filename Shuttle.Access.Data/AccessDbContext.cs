using Microsoft.EntityFrameworkCore;
using Shuttle.Extensions.EFCore;

namespace Shuttle.Access.Data;

public class AccessDbContext : DbContext
{
    private IDisposable? _removeDbContext;

    public AccessDbContext(DbContextOptions<AccessDbContext> options, IDbContextService? dbContextService = null) : base(options)
    {
        _removeDbContext = dbContextService?.Add(this);
    }

    public DbSet<Models.Identity> Identities { get; set; } = null!;
    public DbSet<Models.IdentityRole> IdentityRoles { get; set; } = null!;
    public DbSet<Models.Permission> Permissions { get; set; } = null!;
    public DbSet<Models.Role> Roles { get; set; } = null!;
    public DbSet<Models.RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Models.Session> Sessions { get; set; } = null!;
    public DbSet<Models.SessionPermission> SessionPermissions { get; set; } = null!;
    public DbSet<Models.SessionTokenExchange> SessionTokenExchange { get; set; } = null!;

    public override void Dispose()
    {
        _removeDbContext?.Dispose();
        _removeDbContext = null;

        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        _removeDbContext?.Dispose();
        _removeDbContext = null;

        return base.DisposeAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            entityType.SetTableName(entityType.DisplayName());
        }
    }
}