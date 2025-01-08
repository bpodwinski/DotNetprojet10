using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PatientService.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NoteService.Services
{
    /// <summary>
    /// Service to manage authentication tokens, including generating, storing, and revoking refresh tokens.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<UserDomain> _userManager;
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor for the AuthService, injecting the necessary dependencies.
        /// </summary>
        /// <param name="userManager">ASP.NET Identity UserManager for managing users.</param>
        /// <param name="config">IConfiguration for accessing application settings.</param>
        public AuthService(UserManager<UserDomain> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        /// <summary>
        /// Adds a refresh token and its expiry time for a user and stores them in the AspNetUserTokens table.
        /// </summary>
        /// <param name="user">The user for whom the refresh token is being added.</param>
        /// <param name="refreshToken">The refresh token to be stored.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task AddRefreshToken(UserDomain user, string refreshToken)
        {
            var appName = _config["AppSettings:AppName"];

            // Store the refresh token and its expiration date in the AspNetUserTokens table
            var expiryDate = DateTime.UtcNow.AddDays(7).ToString(); // Expire in 7 days

            // Add or update the refresh token and its expiry in the AspNetUserTokens table
            await _userManager.SetAuthenticationTokenAsync(user, appName, "RefreshToken", refreshToken);
            await _userManager.SetAuthenticationTokenAsync(user, appName, "RefreshTokenExpiry", expiryDate);
        }

        /// <summary>
        /// Retrieves the refresh token for a specific user from the AspNetUserTokens table.
        /// </summary>
        /// <param name="user">The user whose refresh token is being retrieved.</param>
        /// <returns>The refresh token as a string, or null if not found.</returns>
        public async Task<string?> GetRefreshToken(UserDomain user)
        {
            var appName = _config["AppSettings:AppName"];
            return await _userManager.GetAuthenticationTokenAsync(user, appName, "RefreshToken");
        }

        /// <summary>
        /// Retrieves the expiration date of the refresh token for a specific user.
        /// </summary>
        /// <param name="user">The user whose refresh token expiry is being retrieved.</param>
        /// <returns>A DateTime representing the expiry date, or null if not found or invalid.</returns>
        public async Task<DateTime?> GetRefreshTokenExpiry(UserDomain user)
        {
            var appName = _config["AppSettings:AppName"];
            var expiryDateString = await _userManager.GetAuthenticationTokenAsync(user, appName, "RefreshTokenExpiry");
            if (DateTime.TryParse(expiryDateString, out var expiryDate))
            {
                return expiryDate;
            }
            return null;
        }

        /// <summary>
        /// Revokes (removes) the refresh token and its expiration for a specific user.
        /// </summary>
        /// <param name="user">The user whose refresh token is being revoked.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task RevokeRefreshToken(UserDomain user)
        {
            var appName = _config["AppSettings:AppName"];
            await _userManager.RemoveAuthenticationTokenAsync(user, appName, "RefreshToken");
            await _userManager.RemoveAuthenticationTokenAsync(user, appName, "RefreshTokenExpiry");
        }

        /// <summary>
        /// Generates a new refresh token using a secure random number generator.
        /// </summary>
        /// <returns>A Base64-encoded string representing the generated refresh token.</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Generates a new JWT token for the specified user and includes custom claims.
        /// </summary>
        /// <param name="user">The user for whom the JWT is being generated.</param>
        /// <param name="additionalClaims">Additional claims to be included in the token.</param>
        /// <returns>A string representing the generated JWT token.</returns>
        public string GenerateToken(UserDomain user, IList<Claim> additionalClaims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:SecretKey"]!);
            var audience = _config["Jwt:Audience"];
            var issuer = _config["Jwt:Issuer"];
            var tokenExpiryInMinutes = int.TryParse(_config["Jwt:TokenExpiryInMinutes"], out var expiry) ? expiry : 15;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            claims.AddRange(additionalClaims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(tokenExpiryInMinutes),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Extracts the principal from an expired JWT token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>The claims principal extracted from the token.</returns>
        /// <exception cref="SecurityTokenException">Thrown if the token is invalid.</exception>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:SecretKey"]!);
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
            if (!(securityToken is JwtSecurityToken jwtToken) || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
