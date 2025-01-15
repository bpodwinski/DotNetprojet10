using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public class NoteModel
    {
        public string? Id { get; set; }
        public int PatientId { get; set; }
        public string? Note { get; set; }
        public DateTime Date { get; set; }
        public string LocalDate
        {
            get
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");
                return TimeZoneInfo.ConvertTimeFromUtc(Date, timeZone).ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }

    public class NoteAddModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        [MaxLength(500, ErrorMessage = "The note cannot exceed 500 characters")]
        public string? Note { get; set; }
    }
}
