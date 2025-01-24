using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain
{
    public class UserDomain : IdentityUser<int>
    {
        public string FullName { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}