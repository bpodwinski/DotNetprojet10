using NoteService.Domain;

namespace NoteService.Repositories
{
    /// <summary>
    /// Interface for managing Note entities.
    /// Defines the methods for CRUD operations on Note entities.
    /// </summary>
    public interface INoteRepository
    {
        /// <summary>
        /// Asynchronously creates a new Note entity and saves it to the database.
        /// </summary>
        /// <param name="note">The Note entity to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Create(NoteDomain note);

        /// <summary>
        /// Asynchronously deletes a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note entity to delete.</param>
        /// <returns>A task representing the asynchronous operation. The result is the deleted Note entity, or null if not found.</returns>
        Task<NoteDomain?> DeleteById(string id);

        /// <summary>
        /// Asynchronously retrieves a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note entity to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The result is the Note entity, or null if not found.</returns>
        Task<NoteDomain?> GetById(string id);

        /// <summary>
        /// Asynchronously retrieves a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note entity to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The result is the Note entity, or null if not found.</returns>
        Task<NoteDomain?> GetByPatientId(int id);

        /// <summary>
        /// Retrieves all Note entities from the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The result is a list of Note entities.</returns>
        Task<List<NoteDomain>> GetAll();

        /// <summary>
        /// Asynchronously updates an existing Note entity.
        /// </summary>
        /// <param name="note">The Note entity with updated values.</param>
        /// <returns>A task representing the asynchronous operation. The result is the updated Note entity, or null if not found.</returns>
        Task<NoteDomain?> Update(NoteDomain note);
    }
}
