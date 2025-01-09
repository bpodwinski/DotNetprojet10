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
        private const string PatientApiUrl = "http://gateway:5000/patients";

        public PatientRepository(ILogger<PatientRepository> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
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
