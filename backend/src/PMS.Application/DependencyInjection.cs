using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PMS.Application.Features.Auth;
using PMS.Application.Features.Organizations;
using PMS.Application.Features.Properties;
using PMS.Application.Features.Roles;
using PMS.Application.Features.Users;

namespace PMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}
