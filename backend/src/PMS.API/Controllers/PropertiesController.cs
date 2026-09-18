using Microsoft.AspNetCore.Mvc;
using PMS.API.Authorization;
using PMS.Application.Common.Models;
using PMS.Application.Features.Properties;
using PMS.Application.Features.Properties.Dtos;
using PMS.Domain.Constants;

namespace PMS.API.Controllers;

public class PropertiesController(IPropertyService propertyService) : BaseApiController
{
    [HttpGet]
    [HasPermission(PermissionNames.PropertiesView)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await propertyService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionNames.PropertiesView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await propertyService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(PermissionNames.PropertiesCreate)]
    public async Task<IActionResult> Create(CreatePropertyRequest request, CancellationToken cancellationToken)
    {
        var result = await propertyService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" },
            ApiResponse<PropertyDto>.Ok(result, "Property created successfully."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionNames.PropertiesUpdate)]
    public async Task<IActionResult> Update(Guid id, UpdatePropertyRequest request, CancellationToken cancellationToken)
    {
        var result = await propertyService.UpdateAsync(id, request, cancellationToken);
        return Ok(result, "Property updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionNames.PropertiesDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await propertyService.DeleteAsync(id, cancellationToken);
        return Ok<object?>(null, "Property deleted successfully.");
    }
}
