namespace BackEnd.DTOs.Hotel
{
    // What we receive on create (POST)
    public class CreateHotelDto
    {
        public string Name { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
