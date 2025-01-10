using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace NoteService.DTOs
{
    /// <summary>
    /// Data Transfer Object representing note
    /// </summary>
    public class NoteDTO
    {
        /// <summary>
        /// Gets or sets unique identifier note
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets unique identifier patient
        /// </summary>
        public required int PatientId { get; set; }

        /// <summary>
        /// Gets or sets note content
        /// </summary>
        [Required(ErrorMessage = "Note is required.")]
        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
        public required string Note { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        public DateTime? Date { get; set; }
    }
}
