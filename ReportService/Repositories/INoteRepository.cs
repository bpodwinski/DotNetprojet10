using ReportService.Domain;

namespace ReportService.Repositories
{
    /// <summary>
    /// Interface for managing Note entities.
    /// Provides methods for retrieving and managing notes associated with patients.
    /// </summary>
    public interface INoteRepository
    {
        /// <summary>
        /// Asynchronously retrieves a list of Note entities associated with a specific patient ID.
        /// </summary>
        /// <param name="id">The ID of the patient whose notes are to be retrieved.</param>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The result is a list of Note entities, or an empty list if no notes are found.
        /// </returns>
        Task<List<NoteDomain>> GetByPatientId(int id);
    }
}
