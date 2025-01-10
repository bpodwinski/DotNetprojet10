using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AuthService.Domain;
using AuthService.Models;
using AuthService.Services;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers
{
    /// <summary>
    /// Controller to handle authentication operations, including JWT generation, refresh token management, and token revocation.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </remarks>
    [ApiController]
    [Route("auth")]
    public class AuthController(UserManager<UserDomain> userManager, ILogger<AuthController> logger, IJwtService jwtService, IRefreshTokenService refreshtokenService) : ControllerBase
    {
        private readonly UserManager<UserDomain> _userManager = userManager;
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IRefreshTokenService _refreshtokenService = refreshtokenService;

        /// <summary>
        /// Authenticates a user and generates a JWT token and refresh token if the credentials are valid.
        /// </summary>
        /// <param name="model">The login model containing username and password.</param>
        /// <returns>A response containing the JWT token, refresh token, and user ID if successful.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            _logger.LogInformation("Login attempt received.");

            // Validate input
            if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            {
                const string invalidInputMessage = "Invalid login model provided.";
                _logger.LogWarning(invalidInputMessage);
                return BadRequest(new { message = invalidInputMessage });
            }

            try
            {
                _logger.LogInformation("Attempting login for user: {Username}", model.Username);

                // Check if user exists
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    const string invalidUserMessage = "Invalid username or password.";
                    _logger.LogWarning("Login failed: invalid username '{Username}'", model.Username);
                    return Unauthorized(new { message = invalidUserMessage });
                }

                // Validate password
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!isPasswordValid)
                {
                    const string invalidPasswordMessage = "Invalid username or password.";
                    _logger.LogWarning("Login failed: invalid password for user '{Username}'", model.Username);
                    return Unauthorized(new { message = invalidPasswordMessage });
                }

                // Generate and save JWT token
                var userClaims = await _userManager.GetClaimsAsync(user);
                var token = _jwtService.GenerateToken(user, userClaims);
                await _jwtService.SaveToken(user, token);
                _logger.LogInformation("JWT token generated successfully for user '{Username}'", model.Username);

                // Generate refresh token
                var refreshToken = _refreshtokenService.GenerateRefreshToken();
                await _refreshtokenService.AddRefreshToken(user, refreshToken);
                _logger.LogInformation("Refresh token generated and saved successfully for user '{Username}'", model.Username);

                // Respond with tokens
                _logger.LogInformation("Login successful for user: {Username}", model.Username);
                return Ok(new
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    UserId = user.Id
                });
            }
            catch (Exception ex)
            {
                const string internalErrorMessage = "An internal error occurred.";
                _logger.LogError(ex, internalErrorMessage + " User: {Username}", model.Username);
                return StatusCode(500, new { message = internalErrorMessage });
            }
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns>An IActionResult indicating the result of the logout operation.</returns>
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize(policy: "User")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized("User ID not found in claims.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                await _jwtService.RevokeToken(user);
                await _refreshtokenService.RevokeRefreshToken(int.Parse(userId));

                return Ok("User logged out successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred during logout: {ex.Message}");
            }
        }

        [HttpGet("validate-token")]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized("Missing or invalid Authorization header");
                }

                var token = authHeader["Bearer ".Length..];
                var principal = _jwtService.GetPrincipalFromExpiredToken(token);

                var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid token: User ID is missing");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized("Invalid token: User not found");
                }

                // Vérifie si le token a été révoqué
                var isRevoked = await _jwtService.IsTokenRevoked(user, token);
                if (isRevoked)
                {
                    return Unauthorized("Token has been revoked");
                }

                return Ok("Token is valid.");
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = "Invalid token", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during token validation.", details = ex.Message });
            }
        }

        /// <summary>
        /// Refreshes the JWT token using the refresh token.
        /// </summary>
        //[HttpPost]
        //[AllowAnonymous]
        //[Route("refresh-token")]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> RefreshToken([FromBody] TokenRefresh model)
        //{
        //    _logger.LogInformation("Token refresh attempt");

        //    try
        //    {
        //        var principal = _authService.GetPrincipalFromExpiredToken(model.Token);
        //        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //        var user = await _userManager.FindByIdAsync(userId);
        //        if (user == null)
        //        {
        //            _logger.LogWarning("Token refresh failed: user not found with ID '{UserId}'", userId);
        //            return Unauthorized(new { message = "Invalid user" });
        //        }

        //        var storedRefreshToken = await _authService.GetRefreshToken(user);
        //        var refreshTokenExpiry = await _authService.GetRefreshTokenExpiry(user);

        //        if (storedRefreshToken != model.RefreshToken || refreshTokenExpiry <= DateTime.UtcNow)
        //        {
        //            _logger.LogWarning("Token refresh failed: invalid or expired refresh token for user '{UserId}'", userId);
        //            return Unauthorized(new { message = "Invalid or expired refresh token" });
        //        }

        //        var userClaims = await _userManager.GetClaimsAsync(user);
        //        var newJwtToken = _authService.GenerateToken(user, userClaims);
        //        var newRefreshToken = _authService.GenerateRefreshToken();

        //        await _authService.AddRefreshToken(user, newRefreshToken);

        //        _logger.LogInformation("Token refresh successful for user ID: {UserId}", userId);
        //        return Ok(new { Token = newJwtToken, RefreshToken = newRefreshToken });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An internal error occurred during token refresh");
        //        return StatusCode(500, new { message = "An internal error occurred" });
        //    }
        //}

        /// <summary>
        /// Revokes the refresh token for the current user.
        /// </summary>
        //[HttpPost]
        //[Authorize]
        //[Route("revoke-refresh-token")]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> RevokeRefreshToken()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    _logger.LogInformation("Refresh token revocation attempt for user ID: {UserId}", userId);

        //    try
        //    {
        //        var user = await _userManager.FindByIdAsync(userId);
        //        if (user == null)
        //        {
        //            _logger.LogWarning("Refresh token revocation failed: user not found with ID '{UserId}'", userId);
        //            return BadRequest(new { message = "User not found" });
        //        }

        //        await _authService.RevokeRefreshToken(user);
        //        _logger.LogInformation("Refresh token successfully revoked for user ID: {UserId}", userId);

        //        return Ok(new { message = "Refresh token revoked" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An internal error occurred during refresh token revocation for user ID: {UserId}", userId);
        //        return StatusCode(500, new { message = "An internal error occurred" });
        //    }
        //}
    }
}
