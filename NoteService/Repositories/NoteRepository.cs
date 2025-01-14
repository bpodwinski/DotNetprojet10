using MongoDB.Bson;
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
        public async Task<NoteDomain?> DeleteById(ObjectId id)
        {
            try
            {
                var deletedNote = await _notesCollection.FindOneAndDeleteAsync(note => note.Id == id);
                if (deletedNote == null)
                {
                    _logger.LogWarning("Note with ID {Id} not found.", id);
                    return null;
                }

                _logger.LogInformation("Successfully deleted Note with ID {Id}.", id);
                return deletedNote;
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
        public async Task<NoteDomain?> GetById(ObjectId id)
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
        /// Asynchronously retrieves Note entities by Patient ID with optional sorting.
        /// </summary>
        /// <param name="id">The ID of the Patient whose notes are to be retrieved.</param>
        /// <param name="sortBy">The field by which to sort the notes (e.g., "Date", "Content"). Default is "Date".</param>
        /// <param name="isDescending">Specifies whether the sorting should be in descending order. Default is true.</param>
        /// <returns>A sorted list of Note entities, or an empty list if no notes are found.</returns>
        public async Task<List<NoteDomain>> GetByPatientId(int id, string sortBy = "Date", bool isDescending = true)
        {
            try
            {
                var filter = Builders<NoteDomain>.Filter.Eq(note => note.PatientId, id);

                var sortDefinition = isDescending
                    ? Builders<NoteDomain>.Sort.Descending(sortBy)
                    : Builders<NoteDomain>.Sort.Ascending(sortBy);

                var notes = await _notesCollection.Find(filter).Sort(sortDefinition).ToListAsync();

                if (notes.Count == 0)
                {
                    _logger.LogWarning("No notes found for PatientId {Id}.", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved {Count} notes for PatientId {Id}.", notes.Count, id);
                }

                return notes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving notes for PatientId {Id}.", id);
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
