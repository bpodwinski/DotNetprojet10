using Microsoft.AspNetCore.Identity;

namespace PatientService.Domain
{
    public class UserDomain : IdentityUser<int>
    {
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}