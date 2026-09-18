using PMS.Domain.Common;
using PMS.Domain.Enums;

namespace PMS.Domain.Entities;

public class Organization : BaseEntity, IHasTimestamps
{
    public string Name { get; set; } = null!;
    public string? LegalName { get; set; }
    public string Code { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string Currency { get; set; } = "ETB";
    public string Timezone { get; set; } = "Africa/Addis_Ababa";
    public OrganizationStatus Status { get; set; } = OrganizationStatus.Active;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
