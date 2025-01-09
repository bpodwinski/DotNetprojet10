using System.Text.Json;
using ReportService.Domain;

namespace ReportService.Repositories
{
    /// <summary>
    /// Repository class for managing Note entities via the Note API.
    /// </summary>
    public class NoteRepository : INoteRepository
    {
        private readonly ILogger<NoteRepository> _logger;
        private readonly HttpClient _httpClient;
        private const string NoteApiUrl = "http://gateway:5000/notes";

        public NoteRepository(ILogger<NoteRepository> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Asynchronously retrieves a list of Note entities associated with a specific patient ID.
        /// </summary>
        /// <param name="id">The ID of the patient whose notes are to be retrieved.</param>
        /// <returns>
        /// A list of Note entities, or an empty list if no notes are found.
        /// </returns>
        public async Task<List<NoteDomain>> GetByPatientId(string id)
        {
            try
            {
                var requestUrl = $"{NoteApiUrl}/patientid/{id}";

                _logger.LogInformation("Fetching note data from API: {Url}", requestUrl);

                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Deserialize response as a list of NoteDomain
                    var notes = JsonSerializer.Deserialize<List<NoteDomain>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Successfully retrieved notes data for Patient ID {Id}.", id);
                    return notes ?? new List<NoteDomain>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("No notes found for Patient ID {Id}. Returning empty list.", id);
                    return new List<NoteDomain>();
                }
                else
                {
                    _logger.LogError("Failed to retrieve notes data. Status Code: {StatusCode}", response.StatusCode);
                    throw new HttpRequestException($"Error fetching notes data: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving notes data for Patient ID {Id}.", id);
                throw;
            }
        }
    }
}
