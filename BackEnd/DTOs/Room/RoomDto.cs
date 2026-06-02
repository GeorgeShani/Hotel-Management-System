namespace BackEnd.DTOs.Room
{
    // What we return from the API (GET)
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int HotelId { get; set; } // which hotel it belongs to
    }
}
