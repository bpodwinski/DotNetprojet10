using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public class PatientModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$|^\d{10}$", ErrorMessage = "Phone number must be in the format 111-222-3333 or 1112223333.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
