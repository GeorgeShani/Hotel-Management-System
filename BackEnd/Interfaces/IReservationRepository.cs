using BackEnd.Models;

namespace BackEnd.Interfaces
{
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        Task<bool> AreRoomsAvailableAsync(List<int> roomIds, DateTime checkIn, DateTime checkOut);
    }
}
