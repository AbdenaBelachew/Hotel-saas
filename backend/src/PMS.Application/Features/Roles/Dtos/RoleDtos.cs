namespace PMS.Application.Features.Roles.Dtos;

public record RoleDto(Guid Id, string Name, string? Description, bool IsSystemRole, IReadOnlyList<string> Permissions);
