using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PatientService.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a User entity.
    /// </summary>
    public class UserDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the User.
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username for the User.
        /// </summary>
        [Required(ErrorMessage = "The Username field is required.")]
        [MinLength(3, ErrorMessage = "The Username must be at least 3 characters long.")]
        [MaxLength(20, ErrorMessage = "The Username must be no more than 20 characters long.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "The Username can only contain letters and numbers.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for the User.
        /// </summary>
        [Required(ErrorMessage = "The Password field is required.")]
        [MinLength(8, ErrorMessage = "The Password must be at least 8 characters long.")]
        [MaxLength(50, ErrorMessage = "The Password must be no more than 50 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\d\s]).*$", ErrorMessage = "The Password must contain at least one uppercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full name of the User.
        /// </summary>
        [Required(ErrorMessage = "The FullName field is required.")]
        [MaxLength(100, ErrorMessage = "The FullName must be no more than 100 characters long.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role assigned to the User.
        /// </summary>
        [Required(ErrorMessage = "The Role field is required.")]
        [RegularExpression("^(Admin|User)$", ErrorMessage = "The Role must be 'Admin' or 'User'.")]
        public string Role { get; set; } = string.Empty;
    }
}
