using Microsoft.EntityFrameworkCore;
using PMS.Application.Common.Exceptions;
using PMS.Application.Common.Interfaces;
using PMS.Application.Common.Models;
using PMS.Application.Features.Properties.Dtos;
using PMS.Domain.Entities;

namespace PMS.Application.Features.Properties;

public class PropertyService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : IPropertyService
{
    public async Task<PagedResult<PropertyDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        // Tenant scoping is enforced by the DbContext's global query filter on Property.
        var query = unitOfWork.Properties.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(p => p.Name.ToLower().Contains(term.ToLower()) || p.Code.ToLower().Contains(term.ToLower()));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
            _ => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => ToDto(p))
            .ToListAsync(cancellationToken);

        return new PagedResult<PropertyDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PropertyDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var property = await unitOfWork.Properties.Query().FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Property), id);

        return ToDto(property);
    }

    public async Task<PropertyDto> CreateAsync(CreatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        var organizationId = ResolveOrganizationIdForWrite(request.OrganizationId);

        var codeExists = await unitOfWork.Properties.Query()
            .AnyAsync(p => p.OrganizationId == organizationId && p.Code == request.Code, cancellationToken);
        if (codeExists)
        {
            throw new ConflictException($"A property with code '{request.Code}' already exists in this organization.");
        }

        var property = new Property
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = request.Name,
            Code = request.Code,
            PropertyType = request.PropertyType,
            Description = request.Description,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            Phone = request.Phone,
            Email = request.Email,
            Website = request.Website,
            Currency = request.Currency,
            Timezone = request.Timezone,
            CheckInTime = request.CheckInTime,
            CheckOutTime = request.CheckOutTime,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Properties.AddAsync(property, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(property);
    }

    public async Task<PropertyDto> UpdateAsync(Guid id, UpdatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        var property = await unitOfWork.Properties.Query().FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Property), id);

        property.Name = request.Name;
        property.PropertyType = request.PropertyType;
        property.Description = request.Description;
        property.Address = request.Address;
        property.City = request.City;
        property.Country = request.Country;
        property.Phone = request.Phone;
        property.Email = request.Email;
        property.Website = request.Website;
        property.Currency = request.Currency;
        property.Timezone = request.Timezone;
        property.CheckInTime = request.CheckInTime;
        property.CheckOutTime = request.CheckOutTime;
        property.Status = request.Status;
        property.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Properties.Update(property);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(property);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var property = await unitOfWork.Properties.Query().FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Property), id);

        unitOfWork.Properties.Remove(property);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private Guid ResolveOrganizationIdForWrite(Guid? requestedOrganizationId)
    {
        if (currentUser.IsPlatformAdmin)
        {
            return requestedOrganizationId
                ?? throw new ConflictException("Platform administrators must specify an organizationId when creating a property.");
        }

        return currentUser.OrganizationId
            ?? throw new ForbiddenException("Your account is not associated with an organization.");
    }

    private static PropertyDto ToDto(Property p) => new(
        p.Id, p.OrganizationId, p.Name, p.Code, p.PropertyType, p.Description, p.Address, p.City, p.Country,
        p.Phone, p.Email, p.Website, p.LogoUrl, p.Currency, p.Timezone, p.CheckInTime, p.CheckOutTime, p.Status, p.CreatedAt);
}
