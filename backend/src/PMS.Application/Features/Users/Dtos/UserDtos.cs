namespace PMS.Application.Features.Users.Dtos;

public record UserDto(
    Guid Id,
    Guid? OrganizationId,
    string Username,
    string Email,
    string FirstName,
    string? LastName,
    string? Phone,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<Guid> PropertyIds,
    DateTime CreatedAt);

public record CreateUserRequest(
    Guid? OrganizationId,
    string Username,
    string Email,
    string Password,
    string FirstName,
    string? LastName,
    string? Phone,
    string RoleName,
    IReadOnlyList<Guid>? PropertyIds);

public record UpdateUserRequest(
    string FirstName,
    string? LastName,
    string? Phone,
    bool IsActive,
    string RoleName,
    IReadOnlyList<Guid>? PropertyIds);
