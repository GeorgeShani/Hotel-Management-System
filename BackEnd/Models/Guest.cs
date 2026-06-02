namespace BackEnd.Models
{
    public class Guest
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonalNumber { get; set; } = string.Empty; // Unique
        public string PhoneNumber { get; set; } = string.Empty; // Unique

        // Navigation Properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
