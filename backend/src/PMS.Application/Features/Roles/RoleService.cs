using Microsoft.EntityFrameworkCore;
using PMS.Application.Common.Interfaces;
using PMS.Application.Features.Roles.Dtos;

namespace PMS.Application.Features.Roles;

public class RoleService(IUnitOfWork unitOfWork) : IRoleService
{
    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await unitOfWork.Roles.Query()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsSystemRole,
                r.RolePermissions.Select(rp => rp.Permission.Name).OrderBy(n => n).ToList()))
            .ToList();
    }
}
