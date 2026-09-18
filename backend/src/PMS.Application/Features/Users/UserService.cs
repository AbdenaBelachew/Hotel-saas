using Microsoft.EntityFrameworkCore;
using PMS.Application.Common.Exceptions;
using PMS.Application.Common.Interfaces;
using PMS.Application.Common.Models;
using PMS.Application.Features.Users.Dtos;
using PMS.Domain.Constants;
using PMS.Domain.Entities;

namespace PMS.Application.Features.Users;

public class UserService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        // Tenant scoping is enforced by the DbContext's global query filter on User.
        IQueryable<User> query = unitOfWork.Users.Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserProperties);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortDescending
            ? query.OrderByDescending(u => u.CreatedAt)
            : query.OrderBy(u => u.CreatedAt);

        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>
        {
            Items = users.Select(ToDto).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await LoadUserAsync(id, cancellationToken);
        return ToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var organizationId = ResolveOrganizationIdForWrite(request.OrganizationId);

        var role = await GetRoleOrThrowAsync(request.RoleName, cancellationToken);
        EnsureCanAssignRole(role.Name);

        if (await unitOfWork.Users.Query().AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new ConflictException($"A user with email '{request.Email}' already exists.");
        }
        if (await unitOfWork.Users.Query().AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new ConflictException($"A user with username '{request.Username}' already exists.");
        }

        await EnsurePropertiesBelongToOrganizationAsync(request.PropertyIds, organizationId, cancellationToken);

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        foreach (var propertyId in request.PropertyIds ?? Array.Empty<Guid>())
        {
            user.UserProperties.Add(new UserProperty { UserId = user.Id, PropertyId = propertyId });
        }

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        user.UserRoles.First().Role = role;
        return ToDto(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await LoadUserAsync(id, cancellationToken);

        var role = await GetRoleOrThrowAsync(request.RoleName, cancellationToken);
        EnsureCanAssignRole(role.Name);

        await EnsurePropertiesBelongToOrganizationAsync(request.PropertyIds, user.OrganizationId, cancellationToken);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Phone = request.Phone;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        user.UserRoles.Clear();
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });

        user.UserProperties.Clear();
        foreach (var propertyId in request.PropertyIds ?? Array.Empty<Guid>())
        {
            user.UserProperties.Add(new UserProperty { UserId = user.Id, PropertyId = propertyId });
        }

        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == currentUser.UserId)
        {
            throw new ConflictException("You cannot delete your own account.");
        }

        var user = await LoadUserAsync(id, cancellationToken);
        unitOfWork.Users.Remove(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> LoadUserAsync(Guid id, CancellationToken cancellationToken)
    {
        return await unitOfWork.Users.Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserProperties)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);
    }

    private async Task<Role> GetRoleOrThrowAsync(string roleName, CancellationToken cancellationToken)
    {
        return await unitOfWork.Roles.Query().FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
            ?? throw new ConflictException($"Role '{roleName}' does not exist.");
    }

    private void EnsureCanAssignRole(string roleName)
    {
        if (roleName == RoleNames.PlatformAdmin && !currentUser.IsPlatformAdmin)
        {
            throw new ForbiddenException("Only platform administrators can assign the PlatformAdmin role.");
        }
    }

    private async Task EnsurePropertiesBelongToOrganizationAsync(IReadOnlyList<Guid>? propertyIds, Guid? organizationId, CancellationToken cancellationToken)
    {
        if (propertyIds is null || propertyIds.Count == 0)
        {
            return;
        }

        var matchingCount = await unitOfWork.Properties.Query()
            .Where(p => propertyIds.Contains(p.Id) && p.OrganizationId == organizationId)
            .CountAsync(cancellationToken);

        if (matchingCount != propertyIds.Count)
        {
            throw new ConflictException("One or more selected properties do not belong to this organization.");
        }
    }

    private Guid? ResolveOrganizationIdForWrite(Guid? requestedOrganizationId)
    {
        if (currentUser.IsPlatformAdmin)
        {
            return requestedOrganizationId;
        }

        return currentUser.OrganizationId
            ?? throw new ForbiddenException("Your account is not associated with an organization.");
    }

    private static UserDto ToDto(User u) => new(
        u.Id,
        u.OrganizationId,
        u.Username,
        u.Email,
        u.FirstName,
        u.LastName,
        u.Phone,
        u.IsActive,
        u.UserRoles.Select(ur => ur.Role.Name).ToList(),
        u.UserProperties.Select(up => up.PropertyId).ToList(),
        u.CreatedAt);
}
