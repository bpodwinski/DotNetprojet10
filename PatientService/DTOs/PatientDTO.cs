using PatientService.Domain;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PatientService.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a patient's information.
    /// </summary>
    public class PatientDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier of the patient.
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the first name of the patient.
        /// </summary>
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public required string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the patient.
        /// </summary>
        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public required string LastName { get; set; }

        /// <summary>
        /// Gets or sets the date of birth of the patient.
        /// </summary>
        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Gets or sets the gender of the patient.
        /// </summary>

        [Required(ErrorMessage = "Gender is required.")]
        public required Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the address of the patient. This property is optional.
        /// </summary>
        [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the patient. This property is optional.
        /// </summary>
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Phone number must be in the format '100-222-3333'.")]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        public string? PhoneNumber { get; set; }
    }
}
