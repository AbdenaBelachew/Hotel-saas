using PMS.Application.Common.Interfaces;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Persistence;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public IRepository<Organization> Organizations { get; } = new Repository<Organization>(dbContext);
    public IRepository<Property> Properties { get; } = new Repository<Property>(dbContext);
    public IRepository<User> Users { get; } = new Repository<User>(dbContext);
    public IRepository<Role> Roles { get; } = new Repository<Role>(dbContext);
    public IRepository<Permission> Permissions { get; } = new Repository<Permission>(dbContext);
    public IRepository<RefreshToken> RefreshTokens { get; } = new Repository<RefreshToken>(dbContext);
    public IRepository<PasswordResetToken> PasswordResetTokens { get; } = new Repository<PasswordResetToken>(dbContext);
    public IRepository<AuditLog> AuditLogs { get; } = new Repository<AuditLog>(dbContext);
    public IRepository<UserRole> UserRoles { get; } = new Repository<UserRole>(dbContext);
    public IRepository<UserProperty> UserProperties { get; } = new Repository<UserProperty>(dbContext);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
