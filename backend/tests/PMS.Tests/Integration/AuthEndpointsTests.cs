using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PMS.Tests.Integration;

/// <summary>
/// Exercises the real HTTP pipeline (auth, tenant isolation, permission checks) against the
/// database configured via ConnectionStrings__DefaultConnection in the test process's
/// environment - the same database the app seeds in Development (see DataSeeder).
/// </summary>
public class AuthEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development")).CreateClient();

    [Fact]
    public async Task Login_WithSeededPlatformAdmin_ReturnsAccessTokenAndPlatformAdminRole()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "admin@demo.com", password = "ChangeMe123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginEnvelope>();

        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.False(string.IsNullOrWhiteSpace(body.Data!.AccessToken));
        Assert.Contains("PlatformAdmin", body.Data.User.Roles);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "admin@demo.com", password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizations_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/organizations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task NonPlatformAdmin_CannotListAllOrganizations()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "manager@demo.com", password = "ChangeMe123!" });
        var loginBody = await login.Content.ReadFromJsonAsync<LoginEnvelope>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/organizations");
        request.Headers.Add("Authorization", $"Bearer {loginBody!.Data!.AccessToken}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrganizationAdmin_OnlySeesOwnOrganizationsProperties()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "manager@demo.com", password = "ChangeMe123!" });
        var loginBody = await login.Content.ReadFromJsonAsync<LoginEnvelope>();
        var organizationId = loginBody!.Data!.User.OrganizationId;

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/properties");
        request.Headers.Add("Authorization", $"Bearer {loginBody.Data.AccessToken}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PropertiesEnvelope>();

        Assert.NotEmpty(body!.Data!.Items);
        Assert.All(body.Data.Items, p => Assert.Equal(organizationId, p.OrganizationId));
    }

    private record LoginEnvelope(bool Success, LoginData? Data);
    private record LoginData(string AccessToken, UserData User);
    private record UserData(Guid? OrganizationId, IReadOnlyList<string> Roles);

    private record PropertiesEnvelope(bool Success, PagedData? Data);
    private record PagedData(IReadOnlyList<PropertyData> Items);
    private record PropertyData(Guid OrganizationId);
}
