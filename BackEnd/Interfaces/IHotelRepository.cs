using BackEnd.Models;

namespace BackEnd.Interfaces
{
    public interface IHotelRepository : IGenericRepository<Hotel>
    {
        // Filtering method
        Task<IEnumerable<Hotel>> GetFilteredHotelsAsync(string? country, string? city, int? rating);

        // Check: does the hotel have rooms (for delete validation)
        Task<bool> HasRoomsAsync(int hotelId);
    }
}
