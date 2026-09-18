namespace PMS.Application.Common.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? OrganizationId { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlySet<string> Permissions { get; }
    bool IsPlatformAdmin { get; }
    bool HasPermission(string permission);
}
