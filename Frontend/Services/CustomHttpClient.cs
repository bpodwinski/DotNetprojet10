using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;

namespace Frontend.Services
{
    public class CustomHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ProtectedLocalStorage protectedLocalStorage, ILogger<CustomHttpClient> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ProtectedLocalStorage _protectedLocalStorage = protectedLocalStorage;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<CustomHttpClient> _logger = logger;

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            try
            {
                await AddAuthorizationHeader();
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
                await AddAuthorizationHeader();
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

        public async Task<HttpResponseMessage> PutAsync<T>(string requestUri, T content)
        {
            try
            {
                await AddAuthorizationHeader();
                _logger.LogInformation("Sending PUT request to {RequestUri} with content of type {ContentType}", requestUri, typeof(T).Name);
                var response = await _httpClient.PutAsJsonAsync(requestUri, content);
                _logger.LogInformation("Received response for PUT request to {RequestUri}: {StatusCode}", requestUri, response.StatusCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending PUT request to {RequestUri}", requestUri);
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            try
            {
                await AddAuthorizationHeader();
                _logger.LogInformation("Sending DELETE request to {RequestUri}", requestUri);
                var response = await _httpClient.DeleteAsync(requestUri);
                _logger.LogInformation("Received response for DELETE request to {RequestUri}: {StatusCode}", requestUri, response.StatusCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending DELETE request to {RequestUri}", requestUri);
                throw;
            }
        }


        private async Task AddAuthorizationHeader()
        {
            try
            {

                var result = await _protectedLocalStorage.GetAsync<string>("AuthToken");
                if (!result.Success || string.IsNullOrEmpty(result.Value))
                {
                    _logger.LogInformation("No token found in ProtectedLocalStorage. Returning anonymous user");
                }

                var token = result.Value;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation("Authorization header added to the request");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding Authorization header");
                throw;
            }
        }
    }
}
