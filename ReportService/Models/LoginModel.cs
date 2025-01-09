using System.ComponentModel.DataAnnotations;

namespace NoteService.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Le champ Username est requis")]
        [StringLength(50, ErrorMessage = "Le champ Username ne peut pas dépasser 50 caractères.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Le champ Password est requis")]
        [StringLength(100, ErrorMessage = "Le champ Password ne peut pas dépasser 100 caractères.")]
        public required string Password { get; set; }
    }
}
