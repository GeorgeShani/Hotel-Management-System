namespace BackEnd.DTOs.Room
{
    // What we request when creating a room (POST)
    public class CreateRoomDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int HotelId { get; set; } // we must know which hotel we are adding to
    }
}
