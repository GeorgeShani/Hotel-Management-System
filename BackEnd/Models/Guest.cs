namespace BackEnd.Models
{
    public class Guest
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonalNumber { get; set; } = string.Empty; // Unique
        public string PhoneNumber { get; set; } = string.Empty; // Unique

        // Email of the guest. Used to link this record to a login account
        // (ApplicationUser) so the guest can sign in and see reservations made for them.
        public string Email { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
