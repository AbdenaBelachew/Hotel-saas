namespace PMS.Application.Features.Auth.Dtos;

public record LoginRequest(string Email, string Password);

/// <summary>Self-service tenant signup: creates a new Organization plus its first OrganizationAdmin user.</summary>
public record RegisterRequest(
    string OrganizationName,
    string OrganizationCode,
    string Email,
    string Username,
    string Password,
    string FirstName,
    string? LastName);

public record RefreshTokenRequest(string RefreshToken);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserProfileDto User);

public record UserProfileDto(
    Guid Id,
    Guid? OrganizationId,
    string? OrganizationName,
    string Username,
    string Email,
    string FirstName,
    string? LastName,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<Guid> PropertyIds);
