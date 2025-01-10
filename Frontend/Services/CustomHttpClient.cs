using System.Net.Http.Headers;

namespace Frontend.Services
{
    public class CustomHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<CustomHttpClient> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<CustomHttpClient> _logger = logger;

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            try
            {
                AddAuthorizationHeader();
                _logger.LogInformation("Sending GET request to {RequestUri}", requestUri);
                var response = await _httpClient.GetAsync(requestUri);
                _logger.LogInformation("Received response for GET request to {RequestUri}: {StatusCode}", requestUri, response.StatusCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending GET request to {RequestUri}", requestUri);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string requestUri, T content)
        {
            try
            {
                AddAuthorizationHeader();
                _logger.LogInformation("Sending POST request to {RequestUri} with content of type {ContentType}", requestUri, typeof(T).Name);
                var response = await _httpClient.PostAsJsonAsync(requestUri, content);
                _logger.LogInformation("Received response for POST request to {RequestUri}: {StatusCode}", requestUri, response.StatusCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending POST request to {RequestUri}", requestUri);
                throw;
            }
        }

        private void AddAuthorizationHeader()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("Authorization header added to the request.");
                }
                else
                {
                    _logger.LogWarning("No AuthToken found in cookies. Authorization header not added.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding Authorization header.");
                throw;
            }
        }
    }
}
