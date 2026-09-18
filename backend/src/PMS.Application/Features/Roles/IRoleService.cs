using PMS.Application.Features.Roles.Dtos;

namespace PMS.Application.Features.Roles;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
