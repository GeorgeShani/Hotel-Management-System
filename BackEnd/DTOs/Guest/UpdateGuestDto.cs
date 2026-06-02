namespace BackEnd.DTOs.Guest
{
    public class UpdateGuestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // PersonalNumber is not requested on update, as it should not change.
    }
}
