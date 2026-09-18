using Microsoft.EntityFrameworkCore;
using PMS.Application.Common.Exceptions;
using PMS.Application.Common.Interfaces;
using PMS.Application.Common.Models;
using PMS.Application.Features.Organizations.Dtos;
using PMS.Domain.Entities;

namespace PMS.Application.Features.Organizations;

public class OrganizationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : IOrganizationService
{
    public async Task<PagedResult<OrganizationDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsPlatformAdmin)
        {
            throw new ForbiddenException("Only platform administrators can list organizations.");
        }

        var query = unitOfWork.Organizations.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(o => o.Name.ToLower().Contains(term.ToLower()) || o.Code.ToLower().Contains(term.ToLower()));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(o => o.Code) : query.OrderBy(o => o.Code),
            _ => request.SortDescending ? query.OrderByDescending(o => o.Name) : query.OrderBy(o => o.Name)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => ToDto(o, o.Properties.Count))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrganizationDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrganizationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureCanAccess(id);

        var org = await unitOfWork.Organizations.Query()
            .Include(o => o.Properties)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), id);

        return ToDto(org, org.Properties.Count);
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsPlatformAdmin)
        {
            throw new ForbiddenException("Only platform administrators can create organizations.");
        }

        var codeExists = await unitOfWork.Organizations.Query().AnyAsync(o => o.Code == request.Code, cancellationToken);
        if (codeExists)
        {
            throw new ConflictException($"An organization with code '{request.Code}' already exists.");
        }

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            LegalName = request.LegalName,
            Code = request.Code,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            Currency = request.Currency,
            Timezone = request.Timezone,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Organizations.AddAsync(organization, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(organization, 0);
    }

    public async Task<OrganizationDto> UpdateAsync(Guid id, UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        EnsureCanAccess(id);

        var organization = await unitOfWork.Organizations.Query()
            .Include(o => o.Properties)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), id);

        if (!currentUser.IsPlatformAdmin && request.Status != organization.Status)
        {
            throw new ForbiddenException("Only platform administrators can change an organization's status.");
        }

        organization.Name = request.Name;
        organization.LegalName = request.LegalName;
        organization.Email = request.Email;
        organization.Phone = request.Phone;
        organization.Address = request.Address;
        organization.City = request.City;
        organization.Country = request.Country;
        organization.Currency = request.Currency;
        organization.Timezone = request.Timezone;
        organization.Status = request.Status;
        organization.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Organizations.Update(organization);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(organization, organization.Properties.Count);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsPlatformAdmin)
        {
            throw new ForbiddenException("Only platform administrators can delete organizations.");
        }

        var organization = await unitOfWork.Organizations.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), id);

        unitOfWork.Organizations.Remove(organization);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void EnsureCanAccess(Guid organizationId)
    {
        if (currentUser.IsPlatformAdmin)
        {
            return;
        }

        if (currentUser.OrganizationId != organizationId)
        {
            throw new ForbiddenException("You cannot access another organization's data.");
        }
    }

    private static OrganizationDto ToDto(Organization o, int propertyCount) => new(
        o.Id, o.Name, o.LegalName, o.Code, o.Email, o.Phone, o.Address, o.City, o.Country,
        o.LogoUrl, o.Currency, o.Timezone, o.Status, propertyCount, o.CreatedAt);
}
