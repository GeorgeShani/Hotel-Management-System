using BackEnd.Models;

namespace BackEnd.Interfaces
{
    public interface IGuestRepository : IGenericRepository<Guest>
    {
        // Checks whether a specific personal number already exists in the database
        Task<bool> PersonalNumberExistsAsync(string personalNumber);

        // Finds the guest record linked to a login email (used so a Guest can book
        // for themselves without knowing their guest id).
        Task<Guest?> GetByEmailAsync(string email);
    }
}
