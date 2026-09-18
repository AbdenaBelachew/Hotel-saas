using PMS.Application.Common.Models;
using PMS.Application.Features.Users.Dtos;

namespace PMS.Application.Features.Users;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
