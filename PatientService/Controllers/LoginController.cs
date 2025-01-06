using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PatientService.Domain;
using PatientService.Models;
using PatientService.Services;

namespace PatientService.Controllers
{
    /// <summary>
    /// Controller to handle authentication operations, including JWT generation, refresh token management, and token revocation.
    /// </summary>
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserDomain> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        public AuthController(UserManager<UserDomain> userManager, IConfiguration config, ILogger<AuthController> logger, IAuthService authService)
        {
            _userManager = userManager;
            _config = config;
            _logger = logger;
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token and refresh token if the credentials are valid.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            _logger.LogInformation("Login attempt for user: {Username}", model.Username);
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: invalid username '{Username}'", model.Username);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed: invalid password for user '{Username}'", model.Username);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                var userClaims = await _userManager.GetClaimsAsync(user);
                var token = _authService.GenerateToken(user, userClaims);
                var refreshToken = _authService.GenerateRefreshToken();
                await _authService.AddRefreshToken(user, refreshToken);

                _logger.LogInformation("Login successful for user: {Username}", model.Username);
                return Ok(new { Token = token, RefreshToken = refreshToken, UserId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An internal error occurred during login for user '{Username}'", model.Username);
                return StatusCode(500, new { message = "An internal error occurred" });
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
