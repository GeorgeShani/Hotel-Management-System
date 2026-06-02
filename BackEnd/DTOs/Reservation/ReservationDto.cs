namespace BackEnd.DTOs.Reservation
{
    // What we return to the client
    public class ReservationDto
    {
        public int Id { get; set; }
        public int GuestId { get; set; }
        public List<int> RoomIds { get; set; } = new List<int>(); // list of rooms
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; } // calculated price returned to the client
    }
}
