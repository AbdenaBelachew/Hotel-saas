using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PMS.API.Authorization;
using PMS.Domain.Constants;
using PMS.Infrastructure.Services;

namespace PMS.Tests.Unit;

public class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _sut = new();

    private static AuthorizationHandlerContext BuildContext(PermissionRequirement requirement, params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        return new AuthorizationHandlerContext([requirement], principal, resource: null);
    }

    [Fact]
    public async Task Succeeds_WhenUserHasTheExactPermissionClaim()
    {
        var requirement = new PermissionRequirement("properties.view");
        var context = BuildContext(requirement, new Claim(AppClaimTypes.Permission, "properties.view"));

        await _sut.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Fails_WhenUserLacksThePermissionClaim()
    {
        var requirement = new PermissionRequirement("properties.delete");
        var context = BuildContext(requirement, new Claim(AppClaimTypes.Permission, "properties.view"));

        await _sut.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Succeeds_ForPlatformAdmin_RegardlessOfPermissionClaims()
    {
        var requirement = new PermissionRequirement("properties.delete");
        var context = BuildContext(requirement, new Claim(ClaimTypes.Role, RoleNames.PlatformAdmin));

        await _sut.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }
}
