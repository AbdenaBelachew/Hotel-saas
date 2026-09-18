using PMS.Application.Common.Models;
using PMS.Application.Features.Organizations.Dtos;

namespace PMS.Application.Features.Organizations;

public interface IOrganizationService
{
    Task<PagedResult<OrganizationDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<OrganizationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<OrganizationDto> UpdateAsync(Guid id, UpdateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
