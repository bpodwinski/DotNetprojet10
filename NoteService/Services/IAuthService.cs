using PatientService.Domain;
using System.Security.Claims;

namespace NoteService.Services
{
    /// <summary>
    /// Interface to define methods for authentication token management, including JWT and refresh token handling.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Adds a refresh token and its expiry time for a user.
        /// </summary>
        /// <param name="user">The user for whom the refresh token is being added.</param>
        /// <param name="refreshToken">The refresh token to be stored.</param>
        Task AddRefreshToken(UserDomain user, string refreshToken);

        /// <summary>
        /// Retrieves the refresh token for a specific user.
        /// </summary>
        /// <param name="user">The user whose refresh token is being retrieved.</param>
        /// <returns>The refresh token as a string, or null if not found.</returns>
        Task<string?> GetRefreshToken(UserDomain user);

        /// <summary>
        /// Retrieves the expiration date of the refresh token for a specific user.
        /// </summary>
        /// <param name="user">The user whose refresh token expiry is being retrieved.</param>
        /// <returns>A DateTime representing the expiry date, or null if not found or invalid.</returns>
        Task<DateTime?> GetRefreshTokenExpiry(UserDomain user);

        /// <summary>
        /// Revokes (removes) the refresh token and its expiration for a specific user.
        /// </summary>
        /// <param name="user">The user whose refresh token is being revoked.</param>
        Task RevokeRefreshToken(UserDomain user);

        /// <summary>
        /// Generates a new refresh token using a secure random number generator.
        /// </summary>
        /// <returns>A Base64-encoded string representing the generated refresh token.</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Generates a new JWT token for the specified user and includes custom claims.
        /// </summary>
        /// <param name="user">The user for whom the JWT is being generated.</param>
        /// <param name="additionalClaims">Additional claims to be included in the token.</param>
        /// <returns>A string representing the generated JWT token.</returns>
        string GenerateToken(UserDomain user, IList<Claim> additionalClaims);

        /// <summary>
        /// Extracts the principal from an expired JWT token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>The claims principal extracted from the token.</returns>
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
