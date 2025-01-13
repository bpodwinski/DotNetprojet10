using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public class PatientModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1900-01-01", "2100-12-31", ErrorMessage = "Date of Birth must be between 1900 and 2100.")]
        [MaxDate(ErrorMessage = "Date of Birth cannot be in the future.")]
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$|^\d{10}$", ErrorMessage = "Phone number must be in the format 111-222-3333 or 1112223333.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class MaxDateAttribute : ValidationAttribute
    {
        private readonly DateTime _maxDate;

        public MaxDateAttribute()
        {
            _maxDate = DateTime.Today;
        }

        public override bool IsValid(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime <= _maxDate;
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} ne peut pas être dans le futur.";
        }
    }
}
