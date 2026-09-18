using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Application.Common.Exceptions;
using PMS.Application.Common.Interfaces;
using PMS.Application.Features.Auth.Dtos;
using PMS.Domain.Constants;
using PMS.Domain.Entities;

namespace PMS.Application.Features.Auth;

public class AuthService(
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await LoadFullUserUnfilteredAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException("This account has been deactivated.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        unitOfWork.Users.Update(user);

        var result = await IssueTokensAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Organizations.QueryUnfiltered().AnyAsync(o => o.Code == request.OrganizationCode, cancellationToken))
        {
            throw new ConflictException($"Organization code '{request.OrganizationCode}' is already taken.");
        }
        if (await unitOfWork.Users.QueryUnfiltered().AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new ConflictException($"A user with email '{request.Email}' already exists.");
        }
        if (await unitOfWork.Users.QueryUnfiltered().AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new ConflictException($"A user with username '{request.Username}' already exists.");
        }

        var organizationAdminRole = await unitOfWork.Roles.QueryUnfiltered()
            .FirstOrDefaultAsync(r => r.Name == RoleNames.OrganizationAdmin, cancellationToken)
            ?? throw new ConflictException("The OrganizationAdmin role has not been seeded.");

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.OrganizationName,
            Code = request.OrganizationCode,
            CreatedAt = DateTime.UtcNow
        };
        await unitOfWork.Organizations.AddAsync(organization, cancellationToken);

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = organizationAdminRole.Id, Role = organizationAdminRole });

        await unitOfWork.Users.AddAsync(user, cancellationToken);

        var result = await IssueTokensAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<AuthResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);

        var existingToken = await unitOfWork.RefreshTokens.QueryUnfiltered()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            throw new UnauthorizedAppException("The refresh token is invalid or has expired.");
        }

        var user = await LoadFullUserUnfilteredAsync(u => u.Id == existingToken.UserId, cancellationToken)
            ?? throw new UnauthorizedAppException("The refresh token is invalid or has expired.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException("This account has been deactivated.");
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        unitOfWork.RefreshTokens.Update(existingToken);

        var result = await IssueTokensAsync(user, cancellationToken);

        var newToken = await unitOfWork.RefreshTokens.QueryUnfiltered()
            .OrderByDescending(t => t.CreatedAt)
            .FirstAsync(t => t.UserId == user.Id, cancellationToken);
        existingToken.ReplacedByTokenId = newToken.Id;
        unitOfWork.RefreshTokens.Update(existingToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var existingToken = await unitOfWork.RefreshTokens.QueryUnfiltered()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existingToken is not null && existingToken.RevokedAt is null)
        {
            existingToken.RevokedAt = DateTime.UtcNow;
            unitOfWork.RefreshTokens.Update(existingToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.QueryUnfiltered().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        if (!passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            throw new UnauthorizedAppException("The current password is incorrect.");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.QueryUnfiltered().FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Never reveal whether the email exists.
        if (user is null || !user.IsActive)
        {
            return;
        }

        var plainToken = tokenService.GenerateRefreshToken();
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenService.HashRefreshToken(plainToken),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO(Phase 5 - Notifications): deliver this token by email instead of logging it.
        logger.LogInformation(
            "Password reset requested for {Email}. Dev-only token (not emailed yet): {Token}",
            user.Email, plainToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.QueryUnfiltered().FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken)
            ?? throw new UnauthorizedAppException("The reset token is invalid or has expired.");

        var tokenHash = tokenService.HashRefreshToken(request.Token);
        var resetToken = await unitOfWork.PasswordResetTokens.QueryUnfiltered()
            .Where(t => t.UserId == user.Id && t.TokenHash == tokenHash)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (resetToken is null || !resetToken.IsActive)
        {
            throw new UnauthorizedAppException("The reset token is invalid or has expired.");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Users.Update(user);

        resetToken.UsedAt = DateTime.UtcNow;
        unitOfWork.PasswordResetTokens.Update(resetToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await LoadFullUserUnfilteredAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        return ToProfileDto(user);
    }

    private async Task<User?> LoadFullUserUnfilteredAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        return await unitOfWork.Users.QueryUnfiltered()
            .Include(u => u.Organization)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.UserProperties)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    private async Task<AuthResult> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .Distinct()
            .ToList();

        var accessToken = tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshTokenPlain = tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenService.HashRefreshToken(refreshTokenPlain),
            ExpiresAt = DateTime.UtcNow.Add(tokenService.RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow
        };
        await unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        return new AuthResult(
            accessToken,
            refreshTokenPlain,
            DateTime.UtcNow.Add(tokenService.AccessTokenLifetime),
            ToProfileDto(user, roles, permissions));
    }

    private static UserProfileDto ToProfileDto(User user, IReadOnlyList<string>? roles = null, IReadOnlyList<string>? permissions = null) => new(
        user.Id,
        user.OrganizationId,
        user.Organization?.Name,
        user.Username,
        user.Email,
        user.FirstName,
        user.LastName,
        user.IsActive,
        roles ?? user.UserRoles.Select(ur => ur.Role.Name).ToList(),
        permissions ?? user.UserRoles.SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name)).Distinct().ToList(),
        user.UserProperties.Select(up => up.PropertyId).ToList());
}
