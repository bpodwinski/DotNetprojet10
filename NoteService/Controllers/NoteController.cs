using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteService.DTOs;
using NoteService.Services;

namespace NoteService.Controllers
{
    [ApiController]
    [Route("api/notes")]
    public class NoteController(INoteService noteService, ILogger<NoteController> logger) : ControllerBase
    {
        private readonly INoteService _noteService = noteService;
        private readonly ILogger<NoteController> _logger = logger;

        /// <summary>
        /// Retrieves a specific Note by ID.
        /// </summary>
        /// <param name="id">The ID of the Note to retrieve</param>
        /// <returns>A NoteDTO corresponding to the ID</returns>
        /// <response code="200">Returns the NoteDTO</response>
        /// <response code="404">If the Note with the specified ID is not found</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet("id/{id}")]
        [Authorize(policy: "User")]
        [ProducesResponseType(typeof(NoteDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            try
            {
                _logger.LogInformation("Fetching Note item with ID {Id}.", id);
                var note = await _noteService.GetById(id);

                if (note is not null)
                {
                    return Ok(note);
                }

                _logger.LogWarning("Note item with ID {Id} not found.", id);
                return NotFound($"Note with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Note item with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Retrieves all Notes associated with a specific Patient ID.
        /// </summary>
        /// <param name="id">The Patient ID of the Notes to retrieve.</param>
        /// <returns>A list of NoteDTOs corresponding to the Patient ID.</returns>
        /// <response code="200">Returns the list of NoteDTOs.</response>
        /// <response code="404">If no Notes are found for the specified Patient ID.</response>
        /// <response code="500">If an internal error occurs.</response>
        [HttpGet("patientid/{id}")]
        //[Authorize(policy: "User")]
        [ProducesResponseType(typeof(List<NoteDTO>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetByPatientId([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Fetching Note items for Patient ID {Id}.", id);

                var notes = await _noteService.GetByPatientId(id);

                if (notes == null || notes.Count == 0)
                {
                    _logger.LogWarning("No Note items found for Patient ID {Id}.", id);
                    return NotFound($"No notes found for Patient ID {id}.");
                }

                _logger.LogInformation("Successfully fetched {Count} Note items for Patient ID {Id}.", notes.Count, id);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Note items for Patient ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Creates a new Note.
        /// </summary>
        /// <param name="dto">The NoteDTO object to create</param>
        /// <returns>The newly created NoteDTO</returns>
        /// <response code="201">Returns the newly created NoteDTO</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpPost]
        [Authorize(policy: "Admin")]
        [ProducesResponseType(typeof(NoteDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] NoteDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for creating a Note.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Creating a new Note item.");
                var createdNote = await _noteService.Create(dto);
                return CreatedAtAction(nameof(GetByPatientId), new { id = createdNote.Id }, createdNote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a Note item.");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Updates a specific Note.
        /// </summary>
        /// <param name="id">The ID of the Note to update</param>
        /// <param name="dto">The NoteDTO object with updated values</param>
        /// <returns>The updated NoteDTO</returns>
        /// <response code="200">Returns the updated NoteDTO</response>
        /// <response code="404">If the Note with the specified ID is not found</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpPut("{id}")]
        [Authorize(policy: "Admin")]
        [ProducesResponseType(typeof(NoteDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] NoteDTO dto)
        {
            try
            {
                _logger.LogInformation("Updating Note item with ID {Id}.", id);
                var updatedNote = await _noteService.Update(id, dto);
                if (updatedNote is not null)
                {
                    return Ok(updatedNote);
                }
                _logger.LogWarning("Note item with ID {Id} not found for update.", id);
                return NotFound($"Note with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating Note item with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Deletes a specific Note by ID.
        /// </summary>
        /// <param name="id">The ID of the Note to delete</param>
        /// <response code="204">If the Note is successfully deleted</response>
        /// <response code="404">If the Note with the specified ID is not found</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpDelete("{id}")]
        [Authorize(policy: "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                _logger.LogInformation("Deleting Note item with ID {Id}.", id);
                var deletedNote = await _noteService.DeleteById(id);
                if (deletedNote is not null)
                {
                    return NoContent();
                }
                _logger.LogWarning("Note item with ID {Id} not found for deletion.", id);
                return NotFound($"Note with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting Note item with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
