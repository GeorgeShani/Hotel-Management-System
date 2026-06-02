namespace BackEnd.DTOs.Reservation
{
    // What we request from the client when creating a reservation
    public class CreateReservationDto
    {
        public int GuestId { get; set; }
        public List<int> RoomIds { get; set; } = new List<int>(); // client provides the list of rooms
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}
