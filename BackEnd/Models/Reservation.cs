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

        // Navigation Property (M:M)
        public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();
    }
}
