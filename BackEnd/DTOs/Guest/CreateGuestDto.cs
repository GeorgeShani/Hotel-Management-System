namespace BackEnd.DTOs.Guest
{
    public class CreateGuestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonalNumber { get; set; } = string.Empty; // Must be unique
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
