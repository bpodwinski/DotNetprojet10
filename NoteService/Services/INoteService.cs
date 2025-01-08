using MongoDB.Bson;
using NoteService.DTOs;

namespace NoteService.Services
{
    /// <summary>
    /// Interface for Note service to manage CRUD operations on Note entities.
    /// </summary>
    public interface INoteService
    {
        /// <summary>
        /// Asynchronously creates a new Note entity based on the provided DTO.
        /// </summary>
        /// <param name="dto">The DTO containing data for the new Note.</param>
        /// <returns>A task representing the asynchronous operation, with the created NoteDTO.</returns>
        Task<NoteDTO?> Create(NoteDTO dto);

        /// <summary>
        /// Asynchronously deletes a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note to delete.</param>
        /// <returns>A task representing the asynchronous operation, with the deleted NoteDTO, or null if not found.</returns>
        Task<NoteDTO?> DeleteById(string id);

        /// <summary>
        /// Asynchronously retrieves a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with the NoteDTO, or null if not found.</returns>
        Task<NoteDTO?> GetById(string id);

        /// <summary>
        /// Asynchronously retrieves a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with the NoteDTO, or null if not found.</returns>
        Task<List<NoteDTO?>> GetByPatientId(int id);

        /// <summary>
        /// Asynchronously retrieves all Note entities.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of NoteDTOs.</returns>
        Task<List<NoteDTO>> GetAll();

        /// <summary>
        /// Asynchronously updates a Note entity with the provided data.
        /// </summary>
        /// <param name="id">The ID of the Note to update.</param>
        /// <param name="dto">The DTO containing the updated values.</param>
        /// <returns>A task representing the asynchronous operation, with the updated NoteDTO, or null if not found.</returns>
        Task<NoteDTO?> Update(string id, NoteDTO dto);
    }
}
