using ReportService.Domain;

namespace ReportService.Repositories
{
    /// <summary>
    /// Interface for managing Patient entities.
    /// Defines the methods for CRUD operations on Patient entities.
    /// </summary>
    public interface IPatientRepository
    {
        /// <summary>
        /// Asynchronously retrieves a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient entity to retrieve.</param>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The result is the Patient entity, or null if not found.
        /// </returns>
        Task<PatientDomain?> GetById(int id);
    }
}
