using BackEnd.Data;
using BackEnd.Interfaces;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Repositories
{
    public class GuestRepository : GenericRepository<Guest>, IGuestRepository
    {
        public GuestRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> PersonalNumberExistsAsync(string personalNumber)
        {
            return await _context.Guests.AnyAsync(g => g.PersonalNumber == personalNumber);
        }

        public async Task<Guest?> GetByEmailAsync(string email)
        {
            return await _context.Guests
                .FirstOrDefaultAsync(g => g.Email == email);
        }
    }
}
