using Microsoft.AspNetCore.Identity;

namespace BackEnd.Models
{
    // IdentityUser already includes: Id, Email, PasswordHash, PhoneNumber, etc.
    // Here we only add what it doesn't provide by default.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
