namespace BackEnd.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        // Foreign Key
        public int GuestId { get; set; }
        public Guest Guest { get; set; } = null!;

        // Id of the Identity user who created the reservation (its owner).
        // Used so a Guest can only manage their own reservations.
        public string? ApplicationUserId { get; set; }

        // Navigation Property (M:M)
        public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();
    }
}
