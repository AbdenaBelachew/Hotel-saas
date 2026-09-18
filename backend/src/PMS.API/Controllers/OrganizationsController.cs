using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Common.Models;
using PMS.Application.Features.Organizations;
using PMS.Application.Features.Organizations.Dtos;

namespace PMS.API.Controllers;

[Authorize]
public class OrganizationsController(IOrganizationService organizationService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await organizationService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await organizationService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var result = await organizationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" },
            Application.Common.Models.ApiResponse<OrganizationDto>.Ok(result, "Organization created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var result = await organizationService.UpdateAsync(id, request, cancellationToken);
        return Ok(result, "Organization updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await organizationService.DeleteAsync(id, cancellationToken);
        return Ok<object?>(null, "Organization deleted successfully.");
    }
}
