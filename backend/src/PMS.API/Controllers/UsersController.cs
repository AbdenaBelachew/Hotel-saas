using Microsoft.AspNetCore.Mvc;
using PMS.API.Authorization;
using PMS.Application.Common.Models;
using PMS.Application.Features.Users;
using PMS.Application.Features.Users.Dtos;
using PMS.Domain.Constants;

namespace PMS.API.Controllers;

public class UsersController(IUserService userService) : BaseApiController
{
    [HttpGet]
    [HasPermission(PermissionNames.UsersView)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionNames.UsersView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(PermissionNames.UsersCreate)]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" },
            ApiResponse<UserDto>.Ok(result, "User created successfully."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionNames.UsersUpdate)]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.UpdateAsync(id, request, cancellationToken);
        return Ok(result, "User updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionNames.UsersDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(id, cancellationToken);
        return Ok<object?>(null, "User deleted successfully.");
    }
}
