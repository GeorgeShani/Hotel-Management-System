namespace BackEnd.DTOs.Auth
{
    // What we request on registration
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // Everyone who registers is "Guest" by default, but "Manager" or "Admin" can be passed.
        public string Role { get; set; } = "Guest";
    }
}
