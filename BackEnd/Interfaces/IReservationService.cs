using BackEnd.DTOs.Reservation;

namespace BackEnd.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationDto>> GetAllReservationsAsync();
        Task<ReservationDto?> GetReservationByIdAsync(int id);
        Task<ReservationDto> CreateReservationAsync(CreateReservationDto createDto);
        Task UpdateReservationAsync(int id, UpdateReservationDto updateDto);
        Task DeleteReservationAsync(int id);
    }
}
