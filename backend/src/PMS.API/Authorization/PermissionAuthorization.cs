using Microsoft.AspNetCore.Authorization;
using PMS.Infrastructure.Services;

namespace PMS.API.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

/// <summary>Grants access if the JWT carries the required `permission` claim, or if the caller is a PlatformAdmin.</summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var isPlatformAdmin = context.User.IsInRole(PMS.Domain.Constants.RoleNames.PlatformAdmin);
        var hasPermission = context.User.HasClaim(AppClaimTypes.Permission, requirement.Permission);

        if (isPlatformAdmin || hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>Applies a dynamically-named "permission:xxx" policy, resolved by <see cref="PermissionPolicyProvider"/>.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class HasPermissionAttribute(string permission) : AuthorizeAttribute(PolicyPrefix + permission)
{
    public const string PolicyPrefix = "permission:";
}
