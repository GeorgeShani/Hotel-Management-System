using BackEnd.Data;
using BackEnd.Interfaces;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Repositories
{
    public class HotelRepository : GenericRepository<Hotel>, IHotelRepository
    {
        public HotelRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Hotel>> GetFilteredHotelsAsync(string? country, string? city, int? rating)
        {
            var query = _context.Hotels.AsQueryable();

            if (!string.IsNullOrEmpty(country))
                query = query.Where(h => h.Country == country);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(h => h.City == city);

            if (rating.HasValue)
                query = query.Where(h => h.Rating == rating.Value);

            return await query.ToListAsync();
        }

        public async Task<bool> HasRoomsAsync(int hotelId)
        {
            return await _context.Rooms.AnyAsync(r => r.HotelId == hotelId);
        }
    }
}
