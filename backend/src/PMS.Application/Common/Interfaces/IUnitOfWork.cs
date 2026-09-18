using PMS.Domain.Entities;

namespace PMS.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IRepository<Organization> Organizations { get; }
    IRepository<Property> Properties { get; }
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<PasswordResetToken> PasswordResetTokens { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<UserProperty> UserProperties { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
