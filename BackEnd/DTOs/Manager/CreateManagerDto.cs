namespace BackEnd.DTOs.Manager
{
    public class CreateManagerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonalNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int HotelId { get; set; }
    }
}
