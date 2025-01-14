using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReportService.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ElasticsearchClient _client;

        public ElasticsearchService(string elasticsearchUri)
        {
            var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUri))
                .DefaultIndex("medical_notes")
                .ThrowExceptions();
            _client = new ElasticsearchClient(settings);
        }

        public async Task<bool> CreateIndexAsync(string indexName)
        {
            var response = await _client.Indices.ExistsAsync(indexName);

            if (!response.Exists)
            {
                var createResponse = await _client.Indices.CreateAsync<MedicalNote>(index => index
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

        public class MedicalNote
        {
            public string NoteId { get; set; }
            public int PatientId { get; set; }
            public string Note { get; set; }
            public DateTime Date { get; set; }
            public List<string> DetectedTriggers { get; set; } = new();
        }

        public class TriggerTerm
        {
            public string Term { get; set; }
            public string Category { get; set; }
        }

        public async Task IndexDocumentAsync<T>(string indexName, T document)
        {
            var response = await _client.IndexAsync(document, i => i.Index(indexName));

            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to index document: {response.ElasticsearchServerError?.Error.Reason}");
            }
        }

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
                        patientId = patientId
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

        public async Task<List<MedicalNote>> SearchAsync(string indexName, List<string> terms, int patientId)
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
                                patientId = patientId
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

            // Sérialisation en JSON
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Envoi de la requête
            var response = await httpClient.PostAsync(elasticsearchUrl, content);
            response.EnsureSuccessStatusCode();

            // Analyse de la réponse
            var responseBody = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<ElasticsearchSearchResponse<MedicalNote>>(responseBody);

            if (searchResponse?.Hits?.HitList == null)
            {
                return new List<MedicalNote>();
            }

            var result = new List<MedicalNote>();
            if (searchResponse?.Hits?.HitList != null)
            {
                foreach (var hit in searchResponse.Hits.HitList)
                {
                    if (hit.Source != null)
                    {
                        if (hit.Highlight != null && hit.Highlight.ContainsKey("note"))
                        {
                            hit.Source.DetectedTriggers = hit.Highlight["note"];
                        }
                        result.Add(hit.Source);
                    }
                }
            }

            return result;
        }

        public class ElasticsearchSearchResponse<T>
        {
            [JsonPropertyName("hits")]
            public HitsContainer<T> Hits { get; set; }
        }

        public class HitsContainer<T>
        {
            [JsonPropertyName("hits")]
            public List<Hit<T>> HitList { get; set; }
        }

        public class Hit<T>
        {
            [JsonPropertyName("_source")]
            public T Source { get; set; }

            [JsonPropertyName("highlight")]
            public Dictionary<string, List<string>> Highlight { get; set; }
        }

        public async Task BulkIndexTriggerTermsAsync()
        {
            var triggerTerms = new List<TriggerTerm>
            {
                new() { Term = "Hémoglobine A1C", Category = "Biologique" },
                new() { Term = "Microalbumine", Category = "Biologique" },
                new() { Term = "Taille", Category = "Physique" },
                new() { Term = "Poids", Category = "Physique" },
                new() { Term = "Fumeur", Category = "Habitude" },
                new() { Term = "Fumeuse", Category = "Habitude" },
                new() { Term = "Anormal", Category = "État" },
                new() { Term = "Cholestérol", Category = "Biologique" },
                new() { Term = "Vertiges", Category = "Symptôme" },
                new() { Term = "Rechute", Category = "Symptôme" },
                new() { Term = "Réaction", Category = "Symptôme" },
                new() { Term = "Anticorps", Category = "Biologique" }
            };

            var operations = new BulkOperationsCollection(triggerTerms.Select(term => new BulkIndexOperation<TriggerTerm>(term)));

            var bulkRequest = new BulkRequest("trigger_terms_index")
            {
                Operations = operations
            };

            var bulkResponse = await _client.BulkAsync(bulkRequest);

            if (!bulkResponse.IsValidResponse)
            {
                throw new Exception($"Failed to index trigger terms: {bulkResponse.ElasticsearchServerError?.Error.Reason}");
            }
        }
    }
}
