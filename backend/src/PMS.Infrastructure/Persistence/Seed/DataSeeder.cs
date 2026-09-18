using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Constants;
using PMS.Domain.Entities;
using PMS.Domain.Enums;

namespace PMS.Infrastructure.Persistence.Seed;

/// <summary>
/// Development-only convenience seeding, run once at startup. database/schema.sql already seeds
/// the `roles`, `permissions` and `subscription_plans` reference tables; this fills in the
/// role -> permission mapping plus a demo organization/properties/users, matching spec section 35.
/// Idempotent: every write is guarded by an existence check against unfiltered queries, since no
/// authenticated tenant context exists yet at startup.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DataSeeder));

        await SeedRolePermissionsAsync(db);

        var defaultPassword = configuration["SeedData:DefaultPassword"];
        if (string.IsNullOrWhiteSpace(defaultPassword))
        {
            logger.LogWarning("SeedData:DefaultPassword is not configured; skipping demo organization/user seeding.");
            return;
        }

        await SeedDemoTenantAsync(db, passwordHasher, defaultPassword);
    }

    private static async Task SeedRolePermissionsAsync(ApplicationDbContext db)
    {
        var roles = await db.Roles.IgnoreQueryFilters().ToDictionaryAsync(r => r.Name);
        var permissions = await db.Permissions.IgnoreQueryFilters().ToDictionaryAsync(p => p.Name);
        var existingPairs = await db.RolePermissions.IgnoreQueryFilters()
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync();
        var existingSet = existingPairs.Select(x => (x.RoleId, x.PermissionId)).ToHashSet();

        var map = new Dictionary<string, string[]>
        {
            [RoleNames.PlatformAdmin] = permissions.Keys.ToArray(),
            [RoleNames.OrganizationAdmin] = permissions.Keys.ToArray(),
            [RoleNames.PropertyAdmin] =
            [
                PermissionNames.PropertiesView, PermissionNames.PropertiesUpdate,
                PermissionNames.RoomsView, PermissionNames.RoomsCreate, PermissionNames.RoomsUpdate, PermissionNames.RoomsDelete,
                PermissionNames.ReservationsView, PermissionNames.ReservationsCreate, PermissionNames.ReservationsUpdate, PermissionNames.ReservationsCancel,
                PermissionNames.GuestsView, PermissionNames.GuestsCreate, PermissionNames.GuestsUpdate,
                PermissionNames.PaymentsView, PermissionNames.PaymentsCreate, PermissionNames.PaymentsRefund,
                PermissionNames.HousekeepingView, PermissionNames.HousekeepingUpdate,
                PermissionNames.ReportsView, PermissionNames.ReportsExport,
                PermissionNames.UsersView, PermissionNames.UsersCreate, PermissionNames.UsersUpdate
            ],
            [RoleNames.GeneralManager] =
            [
                PermissionNames.PropertiesView, PermissionNames.PropertiesUpdate,
                PermissionNames.RoomsView, PermissionNames.RoomsCreate, PermissionNames.RoomsUpdate, PermissionNames.RoomsDelete,
                PermissionNames.ReservationsView, PermissionNames.ReservationsCreate, PermissionNames.ReservationsUpdate, PermissionNames.ReservationsCancel,
                PermissionNames.GuestsView, PermissionNames.GuestsCreate, PermissionNames.GuestsUpdate,
                PermissionNames.PaymentsView, PermissionNames.PaymentsCreate, PermissionNames.PaymentsRefund,
                PermissionNames.HousekeepingView, PermissionNames.HousekeepingUpdate,
                PermissionNames.ReportsView, PermissionNames.ReportsExport,
                PermissionNames.UsersView, PermissionNames.UsersCreate, PermissionNames.UsersUpdate
            ],
            [RoleNames.Manager] =
            [
                PermissionNames.PropertiesView,
                PermissionNames.RoomsView, PermissionNames.RoomsUpdate,
                PermissionNames.ReservationsView, PermissionNames.ReservationsCreate, PermissionNames.ReservationsUpdate, PermissionNames.ReservationsCancel,
                PermissionNames.GuestsView, PermissionNames.GuestsCreate, PermissionNames.GuestsUpdate,
                PermissionNames.PaymentsView, PermissionNames.PaymentsCreate,
                PermissionNames.HousekeepingView, PermissionNames.HousekeepingUpdate,
                PermissionNames.ReportsView, PermissionNames.ReportsExport,
                PermissionNames.UsersView
            ],
            [RoleNames.FrontDesk] =
            [
                PermissionNames.PropertiesView,
                PermissionNames.RoomsView, PermissionNames.RoomsUpdate,
                PermissionNames.ReservationsView, PermissionNames.ReservationsCreate, PermissionNames.ReservationsUpdate, PermissionNames.ReservationsCancel,
                PermissionNames.GuestsView, PermissionNames.GuestsCreate, PermissionNames.GuestsUpdate,
                PermissionNames.PaymentsView, PermissionNames.PaymentsCreate,
                PermissionNames.HousekeepingView
            ],
            [RoleNames.ReservationAgent] =
            [
                PermissionNames.PropertiesView, PermissionNames.RoomsView,
                PermissionNames.ReservationsView, PermissionNames.ReservationsCreate, PermissionNames.ReservationsUpdate, PermissionNames.ReservationsCancel,
                PermissionNames.GuestsView, PermissionNames.GuestsCreate, PermissionNames.GuestsUpdate,
                PermissionNames.PaymentsView
            ],
            [RoleNames.Accountant] =
            [
                PermissionNames.PropertiesView, PermissionNames.GuestsView,
                PermissionNames.PaymentsView, PermissionNames.PaymentsCreate, PermissionNames.PaymentsRefund,
                PermissionNames.ReportsView, PermissionNames.ReportsExport
            ],
            [RoleNames.Housekeeper] =
            [
                PermissionNames.RoomsView, PermissionNames.HousekeepingView, PermissionNames.HousekeepingUpdate
            ],
            [RoleNames.Maintenance] =
            [
                PermissionNames.RoomsView, PermissionNames.HousekeepingView
            ],
            [RoleNames.ReportViewer] =
            [
                PermissionNames.ReportsView, PermissionNames.ReportsExport
            ]
        };

        foreach (var (roleName, permissionNames) in map)
        {
            if (!roles.TryGetValue(roleName, out var role))
            {
                continue;
            }

            foreach (var permissionName in permissionNames)
            {
                if (!permissions.TryGetValue(permissionName, out var permission))
                {
                    continue;
                }

                if (existingSet.Contains((role.Id, permission.Id)))
                {
                    continue;
                }

                db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
                existingSet.Add((role.Id, permission.Id));
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedDemoTenantAsync(ApplicationDbContext db, IPasswordHasher passwordHasher, string defaultPassword)
    {
        var organization = await db.Organizations.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Code == "DEMO-HG");
        if (organization is null)
        {
            organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = "Demo Hospitality Group",
                Code = "DEMO-HG",
                Email = "info@demo-hospitality.example",
                Currency = "ETB",
                Timezone = "Africa/Addis_Ababa",
                Status = OrganizationStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            db.Organizations.Add(organization);
            await db.SaveChangesAsync();
        }

        var propertyDefs = new (string Name, string Code, PropertyType Type)[]
        {
            ("Grand Hotel Addis", "GHA", PropertyType.Hotel),
            ("Bole Business Hotel", "BBH", PropertyType.Hotel),
            ("Lakeside Resort", "LSR", PropertyType.Resort)
        };

        var properties = await db.Properties.IgnoreQueryFilters()
            .Where(p => p.OrganizationId == organization.Id)
            .ToListAsync();

        foreach (var def in propertyDefs)
        {
            if (properties.Any(p => p.Code == def.Code))
            {
                continue;
            }

            var property = new Property
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Name = def.Name,
                Code = def.Code,
                PropertyType = def.Type,
                Currency = organization.Currency,
                Timezone = organization.Timezone,
                CheckInTime = new TimeOnly(14, 0),
                CheckOutTime = new TimeOnly(12, 0),
                Status = PropertyStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            db.Properties.Add(property);
            properties.Add(property);
        }
        await db.SaveChangesAsync();

        var roles = await db.Roles.IgnoreQueryFilters().ToDictionaryAsync(r => r.Name);
        var firstProperty = properties.First(p => p.Code == "GHA");

        var userDefs = new (string Username, string Email, string FirstName, string LastName, string Role, bool OrgScoped, bool AssignProperty)[]
        {
            ("admin", "admin@demo.com", "Platform", "Admin", RoleNames.PlatformAdmin, false, false),
            ("manager", "manager@demo.com", "Demo", "Manager", RoleNames.OrganizationAdmin, true, false),
            ("frontdesk", "frontdesk@demo.com", "Demo", "FrontDesk", RoleNames.FrontDesk, true, true),
            ("housekeeping", "housekeeping@demo.com", "Demo", "Housekeeper", RoleNames.Housekeeper, true, true),
            ("accountant", "accountant@demo.com", "Demo", "Accountant", RoleNames.Accountant, true, true)
        };

        var existingUsers = await db.Users.IgnoreQueryFilters().ToDictionaryAsync(u => u.Email);

        foreach (var def in userDefs)
        {
            if (existingUsers.ContainsKey(def.Email))
            {
                continue;
            }

            if (!roles.TryGetValue(def.Role, out var role))
            {
                continue;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = def.OrgScoped ? organization.Id : null,
                Username = def.Username,
                Email = def.Email,
                PasswordHash = passwordHasher.Hash(defaultPassword),
                FirstName = def.FirstName,
                LastName = def.LastName,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });

            if (def.AssignProperty)
            {
                user.UserProperties.Add(new UserProperty { UserId = user.Id, PropertyId = firstProperty.Id });
            }

            db.Users.Add(user);
        }

        await db.SaveChangesAsync();
    }
}
