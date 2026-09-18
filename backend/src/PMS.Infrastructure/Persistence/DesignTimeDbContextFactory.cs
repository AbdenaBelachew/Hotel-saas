using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PMS.Application.Common.Interfaces;

namespace PMS.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations` build an ApplicationDbContext without spinning up the full host
/// (which would otherwise run Program.cs's startup migration/seed logic against a real database).
/// Never used at runtime.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=hotel_pms;Username=hotel_pms;Password=change-me-locally";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeCurrentUserService());
    }

    private class DesignTimeCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated => false;
        public Guid? UserId => null;
        public Guid? OrganizationId => null;
        public IReadOnlyList<string> Roles => [];
        public IReadOnlySet<string> Permissions => new HashSet<string>();
        public bool IsPlatformAdmin => true;
        public bool HasPermission(string permission) => true;
    }
}
