namespace PMS.Domain.Constants;

/// <summary>Mirrors the `roles` table seeded by database/schema.sql.</summary>
public static class RoleNames
{
    public const string PlatformAdmin = "PlatformAdmin";
    public const string OrganizationAdmin = "OrganizationAdmin";
    public const string PropertyAdmin = "PropertyAdmin";
    public const string GeneralManager = "GeneralManager";
    public const string FrontDesk = "FrontDesk";
    public const string ReservationAgent = "ReservationAgent";
    public const string Accountant = "Accountant";
    public const string Housekeeper = "Housekeeper";
    public const string Maintenance = "Maintenance";
    public const string Manager = "Manager";
    public const string ReportViewer = "ReportViewer";
}
