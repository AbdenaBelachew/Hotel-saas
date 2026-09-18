using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Common.Models;

namespace PMS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult Ok<T>(T data, string message = "Request completed successfully.") =>
        base.Ok(ApiResponse<T>.Ok(data, message));

    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
