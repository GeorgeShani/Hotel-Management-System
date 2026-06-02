using BackEnd.Models;

namespace BackEnd.Interfaces
{
    public interface IManagerRepository : IGenericRepository<Manager>
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PersonalNumberExistsAsync(string personalNumber);

        // Counts how many managers a specific hotel has
        Task<int> GetManagersCountByHotelIdAsync(int hotelId);
    }
}
