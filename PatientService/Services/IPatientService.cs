using PatientService.DTOs;

namespace PatientService.Services
{
    /// <summary>
    /// Interface for Patient service to manage CRUD operations on Patient entities.
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// Asynchronously creates a new Patient entity based on the provided DTO.
        /// </summary>
        /// <param name="dto">The DTO containing data for the new Patient.</param>
        /// <returns>A task representing the asynchronous operation, with the created PatientDTO.</returns>
        Task<PatientDTO?> Create(PatientDTO dto);

        /// <summary>
        /// Asynchronously deletes a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient to delete.</param>
        /// <returns>A task representing the asynchronous operation, with the deleted PatientDTO, or null if not found.</returns>
        Task<PatientDTO?> DeleteById(int id);

        /// <summary>
        /// Asynchronously retrieves a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with the PatientDTO, or null if not found.</returns>
        Task<PatientDTO?> GetById(int id);

        /// <summary>
        /// Asynchronously retrieves all Patient entities.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of PatientDTOs.</returns>
        Task<List<PatientDTO>> GetAll();

        /// <summary>
        /// Asynchronously updates a Patient entity with the provided data.
        /// </summary>
        /// <param name="id">The ID of the Patient to update.</param>
        /// <param name="dto">The DTO containing the updated values.</param>
        /// <returns>A task representing the asynchronous operation, with the updated PatientDTO, or null if not found.</returns>
        Task<PatientDTO?> Update(int id, PatientDTO dto);
    }
}
