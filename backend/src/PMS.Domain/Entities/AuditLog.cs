using PMS.Domain.Common;

namespace PMS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? OrganizationId { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? UserId { get; set; }

    public string Action { get; set; } = null!;
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }
}
