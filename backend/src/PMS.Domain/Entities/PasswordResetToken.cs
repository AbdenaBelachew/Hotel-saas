using PMS.Domain.Common;

namespace PMS.Domain.Entities;

/// <summary>
/// Additive entity (not in the original database/schema.sql baseline) backing the
/// forgot-password / reset-password flow required by spec section 4. Added via its own
/// EF Core migration on top of the SQL-authored baseline schema.
/// </summary>
public class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsActive => UsedAt is null && ExpiresAt > DateTime.UtcNow;

    public User User { get; set; } = null!;
}
