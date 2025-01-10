using AuthService.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services
{
    /// <summary>
    /// Provides functionality for managing JSON Web Tokens (JWTs), including generating tokens, 
    /// validating tokens, extracting claims, and managing token expiration.
    /// </summary>
    /// <remarks>
    /// This service is designed to handle JWT-based authentication and integrates seamlessly 
    /// with ASP.NET Identity or other authentication systems. It supports token generation with custom claims 
    /// and ensures secure signing using the configured secret key.
    /// </remarks>
    /// <example>
    /// Typical usage involves generating a token for authenticated users and validating incoming tokens 
    /// to extract claims. Use this service as part of a dependency injection setup for secure token management.
    /// </example>
    /// <param name="config">Provides access to application and environment configuration settings.</param>
    /// <param name="logger">Used to log service operations, including token generation, validation, and errors.</param>
    public class JwtService(UserManager<UserDomain> userManager, IConfiguration config, ILogger<JwtService> logger) : IJwtService
    {
        private readonly UserManager<UserDomain> _userManager = userManager;
        private readonly ILogger<JwtService> _logger = logger;
        private readonly int _tokenExpiryInMinutes = int.TryParse(config["Jwt:TokenExpiryInMinutes"], out var expiry) ? expiry : 60;
        private readonly string _loginProviderName = $"{config["AppSettings:AppName"]}_{config["Jwt:LoginProviderName"]}";
        private readonly string _tokenName = "Token";
        private readonly string _tokenExpiryName = "TokenExpiry";

        /// <summary>
        /// Generates a new JWT token for the specified user and includes custom claims.
        /// </summary>
        /// <param name="user">The user for whom the JWT is being generated.</param>
        /// <param name="additionalClaims">Additional claims to be included in the token.</param>
        /// <returns>A string representing the generated JWT token.</returns>
        public string GenerateToken(UserDomain user, IList<Claim> additionalClaims)
        {
            try
            {
                _logger.LogInformation("Generating JWT token for user {UserId}", user.Id);
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!);
                var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
                var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.UserName!),
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Role, user.Role)
                };

                claims.AddRange(additionalClaims);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_tokenExpiryInMinutes),
                    Audience = audience,
                    Issuer = issuer,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                _logger.LogInformation("JWT token generated successfully for user {UserId}", user.Id);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
                throw;
            }
        }

        /// <summary>
        /// Saves a token and its expiration time for a user in the AspNetUserTokens table.
        /// </summary>
        /// <param name="user">The user for whom the token is being saved.</param>
        /// <param name="token">The token to be stored in the user tokens table.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task SaveToken(UserDomain user, string token)
        {
            try
            {
                _logger.LogInformation("Adding token for user {UserId}", user.Id);

                await _userManager.SetAuthenticationTokenAsync(user, _loginProviderName, _tokenName, token);
                var expiryDate = DateTime.UtcNow.AddMinutes(_tokenExpiryInMinutes);
                await _userManager.SetAuthenticationTokenAsync(user, _loginProviderName, _tokenExpiryName, expiryDate.ToString("o"));

                _logger.LogInformation("Refresh token added successfully for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding refresh token for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task RevokeToken(UserDomain user)
        {
            try
            {
                _logger.LogInformation("Revoking token for user {UserId}.", user.Id);
                await _userManager.RemoveAuthenticationTokenAsync(user, _loginProviderName, _tokenName);
                await _userManager.RemoveAuthenticationTokenAsync(user, _loginProviderName, _tokenExpiryName);
                _logger.LogInformation("Token revoked successfully for user {UserId}.", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token for user {UserId}.", user.Id);
                throw;
            }
        }

        public async Task<bool> IsTokenRevoked(UserDomain user, string token)
        {
            try
            {
                _logger.LogInformation("Checking if token is revoked for user {UserId}", user.Id);
                var storedToken = await _userManager.GetAuthenticationTokenAsync(user, _loginProviderName, _tokenName);

                if (storedToken == null || storedToken != token)
                {
                    _logger.LogWarning("Token is revoked for user {UserId}", user.Id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token revocation for user {UserId}", user.Id);
                throw;
            }
        }


        /// <summary>
        /// Extracts the principal from an expired JWT token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>The claims principal extracted from the token.</returns>
        /// <exception cref="SecurityTokenException">Thrown if the token is invalid.</exception>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                _logger.LogInformation("Extracting principal from expired token.");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                _logger.LogInformation("Principal extracted successfully from expired token.");
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting principal from expired token.");
                throw;
            }
        }
    }
}
