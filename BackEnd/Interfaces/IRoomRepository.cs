using BackEnd.Models;

namespace BackEnd.Interfaces
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        // Additional method to fetch the rooms of a specific hotel
        Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(int hotelId);
    }
}
