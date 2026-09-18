using PMS.Domain.Enums;

namespace PMS.Application.Features.Properties.Dtos;

public record PropertyDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Code,
    PropertyType PropertyType,
    string? Description,
    string? Address,
    string? City,
    string? Country,
    string? Phone,
    string? Email,
    string? Website,
    string? LogoUrl,
    string Currency,
    string Timezone,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    PropertyStatus Status,
    DateTime CreatedAt);

public record CreatePropertyRequest(
    Guid? OrganizationId,
    string Name,
    string Code,
    PropertyType PropertyType,
    string? Description,
    string? Address,
    string? City,
    string? Country,
    string? Phone,
    string? Email,
    string? Website,
    string Currency,
    string Timezone,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime);

public record UpdatePropertyRequest(
    string Name,
    PropertyType PropertyType,
    string? Description,
    string? Address,
    string? City,
    string? Country,
    string? Phone,
    string? Email,
    string? Website,
    string Currency,
    string Timezone,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    PropertyStatus Status);
