namespace BackEnd.Models
{
    public class Manager
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonalNumber { get; set; } = string.Empty; // Unique
        public string Email { get; set; } = string.Empty; // Unique
        public string PhoneNumber { get; set; } = string.Empty;

        // Foreign Key
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;
    }
}
