using BackEnd.Models;

namespace BackEnd.Interfaces
{
    public interface IGuestRepository : IGenericRepository<Guest>
    {
        // Checks whether a specific personal number already exists in the database
        Task<bool> PersonalNumberExistsAsync(string personalNumber);
    }
}
