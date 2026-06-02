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
