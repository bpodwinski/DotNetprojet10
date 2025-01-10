using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Services
{
    public class CustomAuthenticationStateProvider(CustomHttpClient customhttpClient, IHttpContextAccessor httpContextAccessor, ILogger<CustomAuthenticationStateProvider> logger) : AuthenticationStateProvider
    {
        private readonly CustomHttpClient _customHttpClient = customhttpClient;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger = logger;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                var token = context?.Request.Cookies["AuthToken"];

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation("No token found in cookies. Returning anonymous user.");
                    return CreateAnonymousState();
                }

                // Validate token with AuthService
                var isValid = await ValidateTokenAsync();
                if (!isValid)
                {
                    _logger.LogWarning("Token is invalid or revoked. Returning anonymous user.");
                    return CreateAnonymousState();
                }

                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "JwtAuth");
                var user = new ClaimsPrincipal(identity);

                _logger.LogInformation("Successfully validated token and created user claims.");
                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token.");
                return CreateAnonymousState();
            }
        }
        private async Task<bool> ValidateTokenAsync()
        {
            try
            {
                _logger.LogInformation("Sending request to AuthService to validate token.");

                // Effectue une requête GET via CustomHttpClient
                var response = await _customHttpClient.GetAsync("/auth/validate-token");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Token is valid.");
                    return true;
                }

                _logger.LogWarning("Token validation failed. Status code: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating token with AuthService.");
                return false;
            }
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            try
            {
                var payload = jwt.Split('.')[1];
                var jsonBytes = Convert.FromBase64String(PadBase64String(payload));
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                if (keyValuePairs == null)
                {
                    _logger.LogWarning("JWT payload is empty. Returning no claims.");
                    return Array.Empty<Claim>();
                }

                var claims = new List<Claim>();
                if (keyValuePairs.ContainsKey(JwtClaimTypes.Role))
                    claims.Add(new Claim(ClaimTypes.Role, keyValuePairs[JwtClaimTypes.Role]?.ToString()!));
                if (keyValuePairs.ContainsKey(JwtClaimTypes.Name))
                    claims.Add(new Claim(ClaimTypes.Name, keyValuePairs[JwtClaimTypes.Name]?.ToString()!));
                if (keyValuePairs.ContainsKey(JwtClaimTypes.Subject))
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, keyValuePairs[JwtClaimTypes.Subject]?.ToString()!));

                _logger.LogInformation("Parsed {Count} claims from token.", claims.Count);
                return claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing claims from JWT.");
                return Array.Empty<Claim>();
            }
        }

        private static string PadBase64String(string base64)
        {
            return base64.Length % 4 == 0 ? base64 : base64.PadRight(base64.Length + (4 - base64.Length % 4), '=');
        }

        public void NotifyUserAuthentication(string token)
        {
            _logger.LogInformation("Notifying user authentication with new token.");
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "JwtAuth");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            _logger.LogInformation("Notifying user logout.");
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        }

        private static AuthenticationState CreateAnonymousState()
        {
            var anonymous = new ClaimsIdentity();
            return new AuthenticationState(new ClaimsPrincipal(anonymous));
        }
    }

    public static class JwtClaimTypes
    {
        public const string Role = "role";
        public const string Name = "name";
        public const string Subject = "sub";
    }
}
