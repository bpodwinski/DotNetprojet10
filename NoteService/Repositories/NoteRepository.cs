using MongoDB.Driver;
using NoteService.Data;
using NoteService.Domain;

namespace NoteService.Repositories
{
    /// <summary>
    /// Repository class for managing Note entities in MongoDB.
    /// </summary>
    public class NoteRepository : INoteRepository
    {
        private readonly IMongoCollection<NoteDomain> _notesCollection;
        private readonly ILogger<NoteRepository> _logger;

        public NoteRepository(MongoDbContext dbContext, ILogger<NoteRepository> logger)
        {
            _notesCollection = dbContext.Notes;
            _logger = logger;
        }

        /// <summary>
        /// Asynchronously creates a new Note entity and saves it to the database.
        /// </summary>
        /// <param name="note">The Note entity to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Create(NoteDomain note)
        {
            try
            {
                await _notesCollection.InsertOneAsync(note);
                _logger.LogInformation("Successfully created Note with ID {NoteId}.", note.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new Note.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously deletes a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note entity to delete.</param>
        /// <returns>The deleted Note entity, or null if not found.</returns>
        public async Task<NoteDomain?> DeleteById(string id)
        {
            try
            {
                var result = await _notesCollection.DeleteOneAsync(note => note.Id == id);
                if (result.DeletedCount == 0)
                {
                    _logger.LogWarning("Attempted to delete Note with ID {Id}, but it was not found.", id);
                    return null;
                }

                _logger.LogInformation("Successfully deleted Note with ID {Id}.", id);
                return null; // MongoDB doesn't return the deleted document by default
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Note with ID {Id}.", id);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note entity to retrieve.</param>
        /// <returns>The Note entity, or null if not found.</returns>
        public async Task<NoteDomain?> GetById(string id)
        {
            try
            {
                var note = await _notesCollection.Find(note => note.Id == id).FirstOrDefaultAsync();
                if (note == null)
                {
                    _logger.LogWarning("Note with ID {Id} was not found.", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved Note with ID {Id}.", id);
                }

                return note;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving Note with ID {Id}.", id);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note entity to retrieve.</param>
        /// <returns>The Note entity, or null if not found.</returns>
        public async Task<NoteDomain?> GetByPatientId(int id)
        {
            try
            {
                var note = await _notesCollection.Find(note => note.PatientId == id).FirstOrDefaultAsync();
                if (note == null)
                {
                    _logger.LogWarning("Note with ID {Id} was not found.", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved Note with ID {Id}.", id);
                }

                return note;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving Note with ID {Id}.", id);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all Note entities from the database.
        /// </summary>
        /// <returns>A list of Note entities.</returns>
        public async Task<List<NoteDomain>> GetAll()
        {
            try
            {
                _logger.LogInformation("Retrieving all Note entities.");
                return await _notesCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all Note entities.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously updates an existing Note entity.
        /// </summary>
        /// <param name="note">The Note entity with updated values.</param>
        /// <returns>The updated Note entity, or null if not found.</returns>
        public async Task<NoteDomain?> Update(NoteDomain note)
        {
            try
            {
                var result = await _notesCollection.ReplaceOneAsync(existingNote => existingNote.Id == note.Id, note);

                if (result.MatchedCount == 0)
                {
                    _logger.LogWarning("Note with ID {NoteId} not found for update.", note.Id);
                    return null;
                }

                _logger.LogInformation("Successfully updated Note with ID {NoteId}.", note.Id);
                return note;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the Note with ID {NoteId}.", note.Id);
                throw;
            }
        }
    }
}
