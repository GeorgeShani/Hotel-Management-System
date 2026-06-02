namespace BackEnd.DTOs.Room
{
    // What we request on update (PUT)
    public class UpdateRoomDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
