using PMS.Domain.Entities;

namespace PMS.Application.Common.Interfaces;

public interface ITokenService
{
    TimeSpan AccessTokenLifetime { get; }
    TimeSpan RefreshTokenLifetime { get; }

    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);

    /// <summary>Cryptographically random opaque refresh token (returned to the client, never stored as-is).</summary>
    string GenerateRefreshToken();

    /// <summary>One-way hash of a refresh token, used for storage/lookup in the database.</summary>
    string HashRefreshToken(string refreshToken);
}
