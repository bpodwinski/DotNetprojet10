using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            var token = context?.Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                // Aucun utilisateur authentifié
                var anonymous = new ClaimsIdentity();
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
            }

            try
            {
                // Décoder le JWT pour obtenir les revendications
                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "JwtAuth");
                var user = new ClaimsPrincipal(identity);

                return Task.FromResult(new AuthenticationState(user));
            }
            catch
            {
                // En cas d'échec, retourner un utilisateur anonyme
                var anonymous = new ClaimsIdentity();
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
            }
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = Convert.FromBase64String(PadBase64String(payload));
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            return keyValuePairs?.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())) ?? Array.Empty<Claim>();
        }

        private static string PadBase64String(string base64)
        {
            // Corrige les chaînes Base64 invalides
            return base64.Length % 4 == 0 ? base64 : base64.PadRight(base64.Length + (4 - base64.Length % 4), '=');
        }
    }
}
