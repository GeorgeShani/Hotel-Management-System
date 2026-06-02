namespace BackEnd.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<Manager> Managers { get; set; } = new List<Manager>();
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
