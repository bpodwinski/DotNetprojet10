using ReportService.Models;

namespace ReportService.Services
{
    public interface IElasticsearchService
    {
        /// <summary>
        /// Checks if an index exists and creates it if necessary.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the index was created or already exists, otherwise false.</returns>
        Task<bool> CreateIndexAsync(string indexName);

        /// <summary>
        /// Indexes a document in Elasticsearch.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="document">The document to be indexed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task IndexDocumentAsync<T>(string indexName, T document);

        /// <summary>
        /// Deletes all notes associated with a specific patient ID from the index.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="patientId">The ID of the patient whose notes should be deleted.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteNotesByPatientIdAsync(string indexName, int patientId);

        /// <summary>
        /// Searches for medical notes in Elasticsearch based on a list of terms and a patient ID.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="terms">A list of terms to search for in the notes.</param>
        /// <param name="patientId">The ID of the patient whose notes are being searched.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching medical notes.</returns>
        Task<List<MedicalNoteModel>> SearchAsync(string indexName, List<string> terms, int patientId);
    }
}
