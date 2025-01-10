using AuthService.Domain;
using System.Security.Claims;

namespace AuthService.Services
{
    /// <summary>
    /// Interface to define methods for authentication token management.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a new JWT token for the specified user and includes custom claims.
        /// </summary>
        /// <param name="user">The user for whom the JWT is being generated.</param>
        /// <param name="additionalClaims">Additional claims to be included in the token.</param>
        /// <returns>A string representing the generated JWT token.</returns>
        string GenerateToken(UserDomain user, IList<Claim> additionalClaims);

        Task SaveToken(UserDomain user, string token);

        Task RevokeToken(UserDomain user);

        Task<bool> IsTokenRevoked(UserDomain user, string token);

        /// <summary>
        /// Extracts the principal from an expired JWT token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>The claims principal extracted from the token.</returns>
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
