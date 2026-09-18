using PMS.Domain.Enums;

namespace PMS.Application.Features.Organizations.Dtos;

public record OrganizationDto(
    Guid Id,
    string Name,
    string? LegalName,
    string Code,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    string? LogoUrl,
    string Currency,
    string Timezone,
    OrganizationStatus Status,
    int PropertyCount,
    DateTime CreatedAt);

public record CreateOrganizationRequest(
    string Name,
    string? LegalName,
    string Code,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    string Currency,
    string Timezone);

public record UpdateOrganizationRequest(
    string Name,
    string? LegalName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    string Currency,
    string Timezone,
    OrganizationStatus Status);
