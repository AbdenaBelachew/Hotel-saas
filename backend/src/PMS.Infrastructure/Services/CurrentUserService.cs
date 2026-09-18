using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Constants;

namespace PMS.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? OrganizationId
    {
        get
        {
            var value = Principal?.FindFirstValue(AppClaimTypes.OrganizationId);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public IReadOnlySet<string> Permissions =>
        Principal?.FindAll(AppClaimTypes.Permission).Select(c => c.Value).ToHashSet() ?? new HashSet<string>();

    public bool IsPlatformAdmin => Roles.Contains(RoleNames.PlatformAdmin);

    public bool HasPermission(string permission) => IsPlatformAdmin || Permissions.Contains(permission);
}

public static class AppClaimTypes
{
    public const string OrganizationId = "org_id";
    public const string Permission = "permission";
}
