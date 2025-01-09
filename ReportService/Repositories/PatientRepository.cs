using System.Text.Json;
using ReportService.Domain;

namespace ReportService.Repositories
{
    /// <summary>
    /// Repository class for managing Patient entities via the Patient API.
    /// </summary>
    public class PatientRepository : IPatientRepository
    {
        private readonly ILogger<PatientRepository> _logger;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string PatientApiUrl = "http://gateway:5000/patients";

        public PatientRepository(ILogger<PatientRepository> logger, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Asynchronously retrieves a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient entity to retrieve.</param>
        /// <returns>
        /// The Patient entity, or null if not found.
        /// </returns>
        public async Task<PatientDomain?> GetById(int id)
        {
            try
            {
                // Récupérer le token JWT depuis le contexte HTTP
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("JWT token is missing in the incoming request.");
                    throw new Exception("JWT token is not available.");
                }

                // Ajouter le token dans les requêtes sortantes
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestUrl = $"{PatientApiUrl}/{id}";

                _logger.LogInformation("Fetching patient data from API: {Url}", requestUrl);

                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Deserialize response
                    var patient = JsonSerializer.Deserialize<PatientDomain>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Successfully retrieved patient data for ID {Id}.", id);
                    return patient;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Patient with ID {Id} not found.", id);
                    return null;
                }
                else
                {
                    _logger.LogError("Failed to retrieve patient data. Status Code: {StatusCode}", response.StatusCode);
                    throw new HttpRequestException($"Error fetching patient data: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving patient data with ID {Id}.", id);
                throw;
            }
        }
    }
}
