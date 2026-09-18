using PMS.Application.Common.Models;
using PMS.Application.Features.Properties.Dtos;

namespace PMS.Application.Features.Properties;

public interface IPropertyService
{
    Task<PagedResult<PropertyDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<PropertyDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PropertyDto> CreateAsync(CreatePropertyRequest request, CancellationToken cancellationToken = default);
    Task<PropertyDto> UpdateAsync(Guid id, UpdatePropertyRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
