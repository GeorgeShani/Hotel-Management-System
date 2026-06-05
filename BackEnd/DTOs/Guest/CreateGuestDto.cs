namespace BackEnd.DTOs.Guest
{
    public class CreateGuestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonalNumber { get; set; } = string.Empty; // Must be unique
        public string PhoneNumber { get; set; } = string.Empty;

        // Set this to the email the guest will use to sign in, so their account
        // can be linked and they can view the reservations made for them.
        public string Email { get; set; } = string.Empty;
    }
}
