namespace PatientService.Domain
{
    public enum Gender
    {
        Male,
        Female
    }

    public class PatientDomain
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public required Gender Gender { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
