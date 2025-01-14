using static ReportService.Services.ElasticsearchService;

namespace ReportService.Services
{
    public interface IElasticsearchService
    {
        /// <summary>
        /// Vérifie si un index existe et le crée si nécessaire.
        /// </summary>
        /// <param name="indexName">Nom de l'index.</param>
        /// <returns>True si l'index a été créé ou existe déjà, sinon False.</returns>
        Task<bool> CreateIndexAsync(string indexName);

        /// <summary>
        /// Indexe un document dans Elasticsearch.
        /// </summary>
        /// <typeparam name="T">Type du document.</typeparam>
        /// <param name="indexName">Nom de l'index.</param>
        /// <param name="document">Document à indexer.</param>
        Task IndexDocumentAsync<T>(string indexName, T document);

        Task DeleteNotesByPatientIdAsync(string indexName, int patientId);

        Task<List<MedicalNote>> SearchAsync(string indexName, List<string> terms, int patientId);

        Task BulkIndexTriggerTermsAsync();
    }
}
