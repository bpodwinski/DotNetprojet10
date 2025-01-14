using Elastic.Clients.Elasticsearch;
using ReportService.Models;
using System.Text;
using System.Text.Json;

namespace ReportService.Services
{
    /// <summary>
    /// Service for interacting with Elasticsearch to manage and search medical notes.
    /// </summary>
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ElasticsearchClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticsearchService"/> class.
        /// </summary>
        /// <param name="elasticsearchUri">The URI of the Elasticsearch instance.</param>
        public ElasticsearchService(string elasticsearchUri)
        {
            var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUri))
                .DefaultIndex("medical_notes")
                .ThrowExceptions();
            _client = new ElasticsearchClient(settings);
        }

        /// <summary>
        /// Checks if the specified index exists in Elasticsearch and creates it if it does not exist.
        /// </summary>
        /// <param name="indexName">The name of the index to check or create.</param>
        /// <returns>True if the index was created or already exists, otherwise false.</returns>
        public async Task<bool> CreateIndexAsync(string indexName)
        {
            var response = await _client.Indices.ExistsAsync(indexName);

            if (!response.Exists)
            {
                var createResponse = await _client.Indices.CreateAsync<MedicalNoteModel>(index => index
                    .Index(indexName)
                    .Mappings(mappings => mappings
                        .Properties(properties => properties
                            .IntegerNumber(p => p.PatientId)
                            .Text(p => p.Note)
                            .Date(p => p.Date)
                        )
                    )
                );

                return createResponse.IsValidResponse;
            }

            return true;
        }

        /// <summary>
        /// Indexes a document in the specified Elasticsearch index.
        /// </summary>
        /// <typeparam name="T">The type of the document to index.</typeparam>
        /// <param name="indexName">The name of the index to store the document in.</param>
        /// <param name="document">The document to index.</param>
        public async Task IndexDocumentAsync<T>(string indexName, T document)
        {
            var response = await _client.IndexAsync(document, i => i.Index(indexName));

            await _client.Indices.RefreshAsync(indexName);

            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to index document: {response.ElasticsearchServerError?.Error.Reason}");
            }
        }

        /// <summary>
        /// Deletes all notes associated with a specific patient ID from the specified Elasticsearch index.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="patientId">The ID of the patient whose notes should be deleted.</param>
        public async Task DeleteNotesByPatientIdAsync(string indexName, int patientId)
        {
            var indexExists = await _client.Indices.ExistsAsync(indexName);
            if (!indexExists.Exists)
            {
                Console.WriteLine($"[Warning] Index '{indexName}' does not exist in Elasticsearch. Skipping delete operation.");
                return;
            }

            var requestBody = new
            {
                query = new
                {
                    term = new
                    {
                        patientId
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var elasticsearchUrl = $"http://ocp10_elasticsearch:9200/{indexName}/_delete_by_query";

            var response = await httpClient.PostAsync(elasticsearchUrl, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Deleted notes: {responseBody}");
        }

        /// <summary>
        /// Searches for medical notes in Elasticsearch based on terms and a patient ID.
        /// </summary>
        /// <param name="indexName">The name of the Elasticsearch index.</param>
        /// <param name="terms">A list of trigger terms to search for.</param>
        /// <param name="patientId">The ID of the patient whose notes are being searched.</param>
        /// <returns>A list of medical notes that match the search criteria.</returns>
        public async Task<List<MedicalNoteModel>> SearchAsync(string indexName, List<string> terms, int patientId)
        {
            using var httpClient = new HttpClient();
            var elasticsearchUrl = $"http://ocp10_elasticsearch:9200/{indexName}/_search";

            var requestBody = new
            {
                query = new
                {
                    @bool = new
                    {
                        must = new
                        {
                            @bool = new
                            {
                                should = terms.Select(term => new
                                {
                                    match = new
                                    {
                                        note = new
                                        {
                                            query = term,
                                            fuzziness = "AUTO"
                                        }
                                    }
                                }).ToArray()
                            }
                        },
                        filter = new
                        {
                            term = new
                            {
                                patientId
                            }
                        }
                    }
                },
                highlight = new
                {
                    fields = new
                    {
                        note = new { }
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(elasticsearchUrl, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<ElasticsearchSearchResponse<MedicalNoteModel>>(responseBody);

            var results = new List<MedicalNoteModel>();
            if (searchResponse?.Hits?.HitList != null)
            {
                foreach (var hit in searchResponse.Hits.HitList)
                {
                    if (hit.Source != null)
                    {
                        hit.Source.HighlightedTriggers = hit.Highlight?.GetValueOrDefault("note") ?? [];
                        results.Add(hit.Source);
                    }
                }
            }

            return results;
        }
    }
}
