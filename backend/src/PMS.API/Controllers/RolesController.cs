using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Features.Roles;

namespace PMS.API.Controllers;

[Authorize]
public class RolesController(IRoleService roleService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await roleService.GetAllAsync(cancellationToken);
        return Ok(result);
    }
}
