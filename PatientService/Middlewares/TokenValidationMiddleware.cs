using System.Net.Http.Headers;

namespace PatientService.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenValidationMiddleware> _logger;

        public TokenValidationMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, ILogger<TokenValidationMiddleware> logger)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader) &&
                authorizationHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorizationHeader.ToString()["Bearer ".Length..];

                try
                {
                    var client = _httpClientFactory.CreateClient("AuthService");
                    var request = new HttpRequestMessage(HttpMethod.Get, "/auth/validate-token");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Token validation failed. Status code: {StatusCode}", response.StatusCode);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized: Token is invalid or revoked");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while validating token with AuthService");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Internal Server Error: Unable to validate token");
                    return;
                }
            }

            await _next(context);
        }
    }
}
