using MongoDB.Bson;
using NoteService.Domain;
using NoteService.DTOs;
using NoteService.Repositories;

namespace NoteService.Services
{
    /// <summary>
    /// Service class for managing operations on Note entities.
    /// </summary>
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;

        public NoteService(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        /// <summary>
        /// Asynchronously creates a new Note entity based on the provided DTO.
        /// </summary>
        /// <param name="dto">The DTO containing data for the new Note.</param>
        /// <returns>The created NoteDTO.</returns>
        public async Task<NoteDTO?> Create(NoteDTO dto)
        {
            try
            {
                var note = ToNote(dto);
                await _noteRepository.Create(note);
                return ToNoteDTO(note);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the Note.", ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note to delete.</param>
        /// <returns>The deleted NoteDTO, or null if not found.</returns>
        public async Task<NoteDTO?> DeleteById(string id)
        {
            try
            {
                var deletedNote = await _noteRepository.DeleteById(id);
                return deletedNote is not null ? ToNoteDTO(deletedNote) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the Note with ID {id}.", ex);
            }
        }

        /// <summary>
        /// Asynchronously retrieves a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note to retrieve.</param>
        /// <returns>The NoteDTO, or null if not found.</returns>
        public async Task<NoteDTO?> GetById(string id)
        {
            try
            {
                var note = await _noteRepository.GetById(id);
                return note is not null ? ToNoteDTO(note) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the Note with ID {id}.", ex);
            }
        }

        /// <summary>
        /// Asynchronously retrieves all Note entities for a specific Patient ID.
        /// </summary>
        /// <param name="id">The ID of the Patient whose notes are to be retrieved.</param>
        /// <returns>A list of NoteDTOs, or an empty list if no notes are found.</returns>
        public async Task<List<NoteDTO>> GetByPatientId(int id)
        {
            try
            {
                var notes = await _noteRepository.GetByPatientId(id);
                var noteDtos = notes.Select(ToNoteDTO).ToList();

                return noteDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving notes for Patient ID {id}.", ex);
            }
        }

        /// <summary>
        /// Retrieves all Note entities and converts them to NoteDTOs.
        /// </summary>
        /// <returns>A list of NoteDTOs.</returns>
        public async Task<List<NoteDTO>> GetAll()
        {
            try
            {
                var notes = await _noteRepository.GetAll();
                return notes.Select(ToNoteDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving all Note entities.", ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a Note entity with the provided data.
        /// </summary>
        /// <param name="id">The ID of the Note to update.</param>
        /// <param name="dto">The DTO containing the updated values.</param>
        /// <returns>The updated NoteDTO, or null if not found.</returns>
        public async Task<NoteDTO?> Update(string id, NoteDTO dto)
        {
            try
            {
                var existingNote = await _noteRepository.GetById(id);
                if (existingNote == null)
                {
                    throw new Exception($"Note with ID {id} not found.");
                }

                var note = ToNote(dto);
                note.Id = id;

                var updatedNote = await _noteRepository.Update(note);
                return updatedNote != null ? ToNoteDTO(updatedNote) : null;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the Note entity.", ex);
            }
        }

        /// <summary>
        /// Converts a NoteDTO to a NoteDomain entity.
        /// </summary>
        /// <param name="dto">The NoteDTO containing data.</param>
        /// <returns>The corresponding NoteDomain entity.</returns>
        private static NoteDomain ToNote(NoteDTO dto) => new()
        {
            Id = dto.Id,
            PatientId = dto.PatientId,
            Note = dto.Note
        };

        /// <summary>
        /// Converts a NoteDomain entity to a NoteDTO.
        /// </summary>
        /// <param name="noteDomain">The NoteDomain entity to convert.</param>
        /// <returns>The corresponding NoteDTO.</returns>
        private static NoteDTO ToNoteDTO(NoteDomain noteDomain) => new()
        {
            Id = noteDomain.Id,
            PatientId = noteDomain.PatientId,
            Note = noteDomain.Note
        };
    }
}
