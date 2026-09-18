using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using PMS.Domain.Entities;
using PMS.Infrastructure.Services;

namespace PMS.Tests.Unit;

public class TokenServiceTests
{
    private readonly TokenService _sut = new(Options.Create(new JwtOptions
    {
        Secret = "unit-test-signing-key-at-least-32-bytes-long!",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    }));

    private static User CreateUser(Guid? organizationId) => new()
    {
        Id = Guid.NewGuid(),
        OrganizationId = organizationId,
        Username = "jdoe",
        Email = "jdoe@example.com",
        PasswordHash = "irrelevant",
        FirstName = "Jane"
    };

    [Fact]
    public void GenerateAccessToken_IncludesRoleAndPermissionClaims()
    {
        var user = CreateUser(Guid.NewGuid());
        var token = _sut.GenerateAccessToken(user, ["FrontDesk"], ["reservations.view", "guests.view"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "FrontDesk");
        Assert.Contains(jwt.Claims, c => c.Type == "permission" && c.Value == "reservations.view");
        Assert.Contains(jwt.Claims, c => c.Type == "permission" && c.Value == "guests.view");
        Assert.Equal("TestIssuer", jwt.Issuer);
    }

    [Fact]
    public void GenerateAccessToken_ForPlatformAdmin_OmitsOrganizationClaim()
    {
        var user = CreateUser(organizationId: null);
        var token = _sut.GenerateAccessToken(user, ["PlatformAdmin"], []);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.DoesNotContain(jwt.Claims, c => c.Type == "org_id");
    }

    [Fact]
    public void GenerateAccessToken_ForTenantUser_IncludesOrganizationClaim()
    {
        var orgId = Guid.NewGuid();
        var user = CreateUser(orgId);
        var token = _sut.GenerateAccessToken(user, ["OrganizationAdmin"], []);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, c => c.Type == "org_id" && c.Value == orgId.ToString());
    }

    [Fact]
    public void HashRefreshToken_IsDeterministic_AndDoesNotReturnThePlainToken()
    {
        var plain = _sut.GenerateRefreshToken();

        var hash1 = _sut.HashRefreshToken(plain);
        var hash2 = _sut.HashRefreshToken(plain);

        Assert.Equal(hash1, hash2);
        Assert.NotEqual(plain, hash1);
    }

    [Fact]
    public void GenerateRefreshToken_ProducesUniqueValues()
    {
        var a = _sut.GenerateRefreshToken();
        var b = _sut.GenerateRefreshToken();

        Assert.NotEqual(a, b);
    }
}
