using BackEnd.Data;
using BackEnd.Interfaces;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Repositories
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(AppDbContext context) : base(context)
        {
        }

        // Eagerly load the join rows and their rooms so RoomIds and TotalPrice
        // can be projected onto the DTO. (FindAsync/plain ToListAsync don't include navigations.)
        public override async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Room)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public override async Task<IReadOnlyList<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Room)
                .ToListAsync();
        }

        public async Task<bool> AreRoomsAvailableAsync(List<int> roomIds, DateTime checkIn, DateTime checkOut)
        {
            // Check whether any of the rooms is already booked for the given dates in the join table
            bool isBooked = await _context.ReservationRooms
                .Include(rr => rr.Reservation)
                .AnyAsync(rr => roomIds.Contains(rr.RoomId) &&
                                (checkIn < rr.Reservation.CheckOutDate && checkOut > rr.Reservation.CheckInDate));

            return !isBooked;
        }
    }
}
