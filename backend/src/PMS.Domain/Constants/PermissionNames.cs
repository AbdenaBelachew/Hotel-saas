namespace PMS.Domain.Constants;

/// <summary>Mirrors the `permissions` table seeded by database/schema.sql.</summary>
public static class PermissionNames
{
    public const string PropertiesView = "properties.view";
    public const string PropertiesCreate = "properties.create";
    public const string PropertiesUpdate = "properties.update";
    public const string PropertiesDelete = "properties.delete";

    public const string RoomsView = "rooms.view";
    public const string RoomsCreate = "rooms.create";
    public const string RoomsUpdate = "rooms.update";
    public const string RoomsDelete = "rooms.delete";

    public const string ReservationsView = "reservations.view";
    public const string ReservationsCreate = "reservations.create";
    public const string ReservationsUpdate = "reservations.update";
    public const string ReservationsCancel = "reservations.cancel";

    public const string GuestsView = "guests.view";
    public const string GuestsCreate = "guests.create";
    public const string GuestsUpdate = "guests.update";

    public const string PaymentsView = "payments.view";
    public const string PaymentsCreate = "payments.create";
    public const string PaymentsRefund = "payments.refund";

    public const string HousekeepingView = "housekeeping.view";
    public const string HousekeepingUpdate = "housekeeping.update";

    public const string ReportsView = "reports.view";
    public const string ReportsExport = "reports.export";

    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";
}
