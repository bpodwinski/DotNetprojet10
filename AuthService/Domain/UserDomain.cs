using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain
{
    public class UserDomain : IdentityUser<int>
    {
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}