using PatientService.Domain;

namespace PatientService.Repositories
{
    /// <summary>
    /// Interface for managing Patient entities.
    /// Defines the methods for CRUD operations on Patient entities.
    /// </summary>
    public interface IPatientRepository
    {
        /// <summary>
        /// Asynchronously creates a new Patient entity and saves it to the database.
        /// </summary>
        /// <param name="patient">The Patient entity to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Create(PatientDomain patient);

        /// <summary>
        /// Asynchronously deletes a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient entity to delete.</param>
        /// <returns>The deleted Patient entity, or null if not found.</returns>
        Task<PatientDomain?> DeleteById(int id);

        /// <summary>
        /// Asynchronously retrieves a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient entity to retrieve.</param>
        /// <returns>The Patient entity, or null if not found.</returns>
        Task<PatientDomain?> GetById(int id);

        /// <summary>
        /// Retrieves all Patient entities from the database.
        /// </summary>
        /// <returns>An IQueryable of Patient entities.</returns>
        IQueryable<PatientDomain> GetAll();

        /// <summary>
        /// Asynchronously updates an existing Patient entity and saves changes to the database.
        /// </summary>
        /// <param name="patient">The Patient entity with updated values.</param>
        /// <returns>The updated Patient entity, or null if not found.</returns>
        Task<PatientDomain?> Update(PatientDomain patient);
    }
}
