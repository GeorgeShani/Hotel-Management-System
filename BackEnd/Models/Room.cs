namespace BackEnd.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; } // > 0

        // Foreign Key
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;

        // Navigation Properties (M:M)
        public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();
    }
}
