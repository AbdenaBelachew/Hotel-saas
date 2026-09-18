using PMS.Domain.Common;
using PMS.Domain.Enums;

namespace PMS.Domain.Entities;

public class Property : BaseEntity, IHasTimestamps
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public PropertyType PropertyType { get; set; } = PropertyType.Hotel;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string Currency { get; set; } = "ETB";
    public string Timezone { get; set; } = "Africa/Addis_Ababa";
    public TimeOnly CheckInTime { get; set; } = new(14, 0);
    public TimeOnly CheckOutTime { get; set; } = new(12, 0);
    public PropertyStatus Status { get; set; } = PropertyStatus.Active;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<UserProperty> UserProperties { get; set; } = new List<UserProperty>();
}
