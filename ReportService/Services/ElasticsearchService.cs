using Elastic.Clients.Elasticsearch;
using ReportService.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
                            },
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
            Console.WriteLine($"Elasticsearch request body: {jsonContent}");
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(elasticsearchUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Elasticsearch error response: {errorContent}");
                throw new Exception($"Elasticsearch error: {errorContent}");
            }
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<ElasticsearchSearchResponse<MedicalNoteModel>>(responseBody);

            var results = new List<MedicalNoteModel>();

            var negativeKeywords = new List<string>
            {
                "ne", "pas", "jamais", "rien", "aucun", "aucuns", "aucune", "aucunes", "nul", "nulle",
                "sans", "ni", "plus", "aucun signe", "pas de signe", "aucune trace",
                "pas de trace"
            };

            var negativeContextPatterns = new List<string>
            {
                @"\banomalie(s)?\b",         // "anomalie" ou "anomalies"
                @"\bproblème(s)?\b",         // "problème" ou "problèmes"
                @"\bdysfonctionnement(s)?\b",// "dysfonctionnement" ou "dysfonctionnements"
                @"\baltération(s)?\b",       // "altération" ou "altérations"
                @"\bdégradation(s)?\b",      // "dégradation" ou "dégradations"
                @"\birrégularité(s)?\b",     // "irrégularité" ou "irrégularités"
                @"\bdéfaut(s)?\b",           // "défaut" ou "défauts"
                @"\btrouble(s)?\b"           // "trouble" ou "troubles"
            };


            if (searchResponse?.Hits?.HitList != null)
            {
                foreach (var hit in searchResponse.Hits.HitList)
                {
                    if (hit.Source != null)
                    {
                        // Ajoute les déclencheurs surlignés
                        hit.Source.HighlightedTriggers = hit.Highlight?.GetValueOrDefault("note") ?? [];

                        var validTriggers = new List<string>();

                        foreach (var highlightedTrigger in hit.Source.HighlightedTriggers)
                        {
                            // Fusionner les balises <em> consécutives dans le texte pour éviter de découper les déclencheurs complexes
                            var mergedTrigger = Regex.Replace(highlightedTrigger, @"</em>\s*<em>", " ");

                            // Extraire les mots entre <em> et </em>
                            var matches = Regex.Matches(mergedTrigger, @"<em>(.*?)</em>");
                            var triggers = matches.Cast<Match>().Select(m => m.Groups[1].Value).ToList();

                            foreach (var trigger in triggers)
                            {
                                if (trigger.Equals("normal", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine($"Excluded Trigger: {trigger} (invalid match for 'anormal').");
                                    continue;
                                }

                                // Vérifie la présence de mots négatifs dans le contexte
                                Console.WriteLine($"Processing Highlighted Trigger: {highlightedTrigger}");
                                Console.WriteLine($"Checking Trigger: {trigger} against negative keywords.");

                                if (!negativeKeywords.Any(negative =>
                                    Regex.IsMatch(highlightedTrigger, $@"\b{negative}\b\s*(de|du|des|d')?\s*<em>{Regex.Escape(trigger)}</em>", RegexOptions.IgnoreCase) || // Avant le déclencheur
                                    Regex.IsMatch(highlightedTrigger, $@"<em>{Regex.Escape(trigger)}</em>\s*(de|du|des|d')?\s*\b{negative}\b", RegexOptions.IgnoreCase) ||  // Après le déclencheur
                                    negativeContextPatterns.Any(pattern =>
                                        Regex.IsMatch(highlightedTrigger, $@"\b{negative}\b.*{pattern}.*<em>{Regex.Escape(trigger)}</em>", RegexOptions.IgnoreCase) || // Avant avec contexte
                                        Regex.IsMatch(highlightedTrigger, $@"<em>{Regex.Escape(trigger)}</em>.*\b{negative}\b.*{pattern}", RegexOptions.IgnoreCase) // Après avec contexte
                                    )))
                                {
                                    Console.WriteLine($"Validated Trigger: {trigger}");
                                    validTriggers.Add(trigger);
                                }
                                else
                                {
                                    Console.WriteLine($"Excluded Trigger: {trigger} due to context in: {highlightedTrigger}");
                                }
                            }
                        }

                        // Met à jour les déclencheurs valides
                        hit.Source.HighlightedTriggers = validTriggers.Distinct().ToList();
                        results.Add(hit.Source);
                    }
                }
            }

            return results;
        }
    }
}
