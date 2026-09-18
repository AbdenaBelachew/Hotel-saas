using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUser)
    : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserProperty> UserProperties => Set<UserProperty>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Tenant isolation (spec rules #11/#12): a non-platform-admin caller can only ever
        // see rows belonging to their own organization, enforced at the query root itself.
        modelBuilder.Entity<Organization>().HasQueryFilter(o => currentUser.IsPlatformAdmin || o.Id == currentUser.OrganizationId);
        modelBuilder.Entity<Property>().HasQueryFilter(p => currentUser.IsPlatformAdmin || p.OrganizationId == currentUser.OrganizationId);
        modelBuilder.Entity<User>().HasQueryFilter(u => currentUser.IsPlatformAdmin || u.OrganizationId == currentUser.OrganizationId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = BuildAuditEntries();
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Cross-cutting audit trail (spec section 20): every Add/Modify/Delete on a tenant-owned
    /// entity is recorded automatically, so individual services never have to remember to log it.
    /// </summary>
    private List<AuditLog> BuildAuditEntries()
    {
        var entries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not (Organization or Property or User))
            {
                continue;
            }

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            var entityName = entry.Entity.GetType().Name;
            var entityId = entry.Property("Id").CurrentValue as Guid?;

            var oldValues = entry.State == EntityState.Added
                ? null
                : JsonSerializer.Serialize(entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p]));

            var newValues = entry.State == EntityState.Deleted
                ? null
                : JsonSerializer.Serialize(entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p]));

            entries.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = currentUser.OrganizationId,
                UserId = currentUser.UserId,
                Action = $"{entityName}.{entry.State}",
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                CreatedAt = DateTime.UtcNow
            });
        }

        return entries;
    }
}
