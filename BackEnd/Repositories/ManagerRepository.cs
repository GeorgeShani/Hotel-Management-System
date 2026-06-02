using BackEnd.Data;
using BackEnd.Interfaces;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Repositories
{
    public class ManagerRepository : GenericRepository<Manager>, IManagerRepository
    {
        public ManagerRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Managers.AnyAsync(m => m.Email == email);
        }

        public async Task<bool> PersonalNumberExistsAsync(string personalNumber)
        {
            return await _context.Managers.AnyAsync(m => m.PersonalNumber == personalNumber);
        }

        public async Task<int> GetManagersCountByHotelIdAsync(int hotelId)
        {
            return await _context.Managers.CountAsync(m => m.HotelId == hotelId);
        }
    }
}
