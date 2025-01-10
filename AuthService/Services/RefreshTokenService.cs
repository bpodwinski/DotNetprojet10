using AuthService.Domain;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace AuthService.Services
{
    /// <summary>
    /// Provides functionality for managing refresh tokens, including storing, retrieving, 
    /// validating, and revoking tokens for users.
    /// </summary>
    /// <remarks>
    /// This service integrates with ASP.NET Identity to support token-based authentication workflows. 
    /// It ensures secure and efficient handling of refresh tokens to facilitate session continuation 
    /// without requiring frequent re-authentication.
    /// </remarks>
    /// <example>
    /// This service can be used to issue a refresh token during user authentication, 
    /// validate the refresh token when renewing a JWT, and revoke tokens when needed.
    /// Typical usage involves integrating it with an authentication controller to manage user sessions securely.
    /// </example>
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly UserManager<UserDomain> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly string _loginProviderName;
        private readonly string _refreshtoken;
        private readonly string _refreshtokenexpiry;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenService"/> class with the specified dependencies.
        /// </summary>
        /// <param name="userManager">UserManager for managing user-related operations, such as retrieving and updating users.</param>
        /// <param name="config">Configuration for accessing application and environment settings.</param>
        /// <param name="logger">Logger for tracking service operations and error handling.</param>
        public RefreshTokenService(UserManager<UserDomain> userManager, IConfiguration config, ILogger<RefreshTokenService> logger)
        {
            _userManager = userManager;
            _config = config;
            _logger = logger;
            _loginProviderName = $"{_config["AppSettings:AppName"]}_{config["Jwt:LoginProviderName"]}";
            _refreshtoken = "RefreshToken";
            _refreshtokenexpiry = "RefreshTokenExpiry";
        }

        /// <summary>
        /// Adds a refresh token and its expiry time for a user and stores them in the AspNetUserTokens table.
        /// </summary>
        /// <param name="user">The user for whom the refresh token is being added.</param>
        /// <param name="refreshToken">The refresh token to be stored.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task AddRefreshToken(UserDomain user, string refreshToken)
        {
            try
            {
                var expiryDate = DateTime.UtcNow.AddDays(7).ToString();
                _logger.LogInformation("Adding refresh token for user {UserName}", user.UserName);

                await _userManager.SetAuthenticationTokenAsync(user, _loginProviderName, _refreshtoken, refreshToken);
                await _userManager.SetAuthenticationTokenAsync(user, _loginProviderName, _refreshtokenexpiry, expiryDate);

                _logger.LogInformation("Refresh token added successfully for user {UserName}", user.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding refresh token for user {UserName}", user.UserName);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the refresh token for a specific user from the AspNetUserTokens table.
        /// </summary>
        /// <param name="user">The user whose refresh token is being retrieved.</param>
        /// <returns>The refresh token as a string, or null if not found.</returns>
        public async Task<string?> GetRefreshToken(UserDomain user)
        {
            try
            {
                _logger.LogInformation("Retrieving refresh token for user {UserName}", user.UserName);
                return await _userManager.GetAuthenticationTokenAsync(user, _loginProviderName, _refreshtoken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refresh token for user {UserName}", user.UserName);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the expiration date of the refresh token for a specific user.
        /// </summary>
        /// <param name="user">The user whose refresh token expiry is being retrieved.</param>
        /// <returns>A DateTime representing the expiry date, or null if not found or invalid.</returns>
        public async Task<DateTime?> GetRefreshTokenExpiry(UserDomain user)
        {
            try
            {
                _logger.LogInformation("Retrieving refresh token expiry for user {UserName}", user.UserName);
                var expiryDateString = await _userManager.GetAuthenticationTokenAsync(user, _loginProviderName, _refreshtokenexpiry);

                if (DateTime.TryParse(expiryDateString, out var expiryDate))
                {
                    return expiryDate;
                }

                _logger.LogWarning("Invalid refresh token expiry date for user {UserName}", user.UserName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refresh token expiry for user {UserName}", user.UserName);
                throw;
            }
        }

        /// <summary>
        /// Revokes (removes) the refresh token and its expiration for a specific user.
        /// </summary>
        /// <param name="userId">The user ID whose refresh token is being revoked.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task RevokeRefreshToken(int userId)
        {
            try
            {
                _logger.LogInformation("Logging out user with ID {UserId}", userId);

                // Retrieve the user
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found during logout.", userId);
                    throw new InvalidOperationException("User not found.");
                }

                _logger.LogInformation("Revoking refresh token for user {UserId}", userId);
                await _userManager.RemoveAuthenticationTokenAsync(user, _loginProviderName, _refreshtoken);
                await _userManager.RemoveAuthenticationTokenAsync(user, _loginProviderName, _refreshtokenexpiry);

                _logger.LogInformation("Refresh token revoked successfully for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout for user with ID {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Generates a new refresh token using a secure random number generator.
        /// </summary>
        /// <returns>A Base64-encoded string representing the generated refresh token.</returns>
        public string GenerateRefreshToken()
        {
            try
            {
                _logger.LogInformation("Generating new refresh token.");
                var randomNumber = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token.");
                throw;
            }
        }
    }
}
