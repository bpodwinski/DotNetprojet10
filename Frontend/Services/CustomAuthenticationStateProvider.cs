using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Frontend.Services
{
    public class CustomAuthenticationStateProvider(CustomHttpClient customHttpClient, ProtectedLocalStorage protectedLocalStorage, ILogger<CustomAuthenticationStateProvider> logger) : AuthenticationStateProvider
    {
        private readonly CustomHttpClient _customHttpClient = customHttpClient;
        private readonly ProtectedLocalStorage _protectedLocalStorage = protectedLocalStorage;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger = logger;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Récupérer le token depuis ProtectedLocalStorage
                var result = await _protectedLocalStorage.GetAsync<string>("AuthToken");
                if (!result.Success || string.IsNullOrEmpty(result.Value))
                {
                    _logger.LogInformation("No token found in ProtectedLocalStorage. Returning anonymous user.");
                    return CreateAnonymousState();
                }

                var token = result.Value;

                // Valider le token avec AuthService
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

        public async Task NotifyUserAuthenticationAsync(string token)
        {
            try
            {
                _logger.LogInformation("Storing token in ProtectedLocalStorage.");
                await _protectedLocalStorage.SetAsync("AuthToken", token);

                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "JwtAuth");
                var user = new ClaimsPrincipal(identity);

                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing token in ProtectedLocalStorage.");
            }
        }

        public async Task NotifyUserLogout()
        {
            try
            {
                _logger.LogInformation("Removing token from ProtectedLocalStorage and notifying logout.");
                await _protectedLocalStorage.DeleteAsync("AuthToken");

                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing token from ProtectedLocalStorage.");
            }
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
